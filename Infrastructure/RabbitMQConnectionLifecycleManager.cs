using HartsyRabbit.Configuration;
using HartsyRabbit.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;

namespace HartsyRabbit.Infrastructure;

public sealed class RabbitMQConnectionLifecycleManager : IRabbitMQConnectionLifecycleManager, IDisposable
{
    private readonly MessageBusConfiguration _configuration;
    private readonly IMessageBusLogger _logger;

    private IConnection? _connection;
    private IChannel? _publishChannel;

    private readonly ConcurrentDictionary<string, IChannel> _consumerChannels = new();
    private readonly ConcurrentDictionary<string, string> _consumerTags = new();

    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private bool _started;
    private bool _disposed;

    public RabbitMQConnectionLifecycleManager(IOptions<MessageBusConfiguration> configuration, IMessageBusLogger logger)
    {
        _configuration = configuration.Value ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RabbitMQConnectionLifecycleManager));
        if (_started) return;

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_started) return;
            await EnsureConnectionAsync(cancellationToken);
            _started = true;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            _started = false;

            foreach ((string queue, IChannel ch) in _consumerChannels)
            {
                try
                {
                    if (_consumerTags.TryGetValue(queue, out string? tag) && !string.IsNullOrWhiteSpace(tag))
                    {
                        await ch.BasicCancelAsync(tag, cancellationToken: cancellationToken);
                    }
                }
                catch
                {
                }

                try
                {
                    await ch.CloseAsync(cancellationToken: cancellationToken);
                }
                catch
                {
                }

                ch.Dispose();
            }

            _consumerChannels.Clear();
            _consumerTags.Clear();

            if (_publishChannel != null)
            {
                try { await _publishChannel.CloseAsync(cancellationToken: cancellationToken); } catch { }
                _publishChannel.Dispose();
                _publishChannel = null;
            }

            if (_connection != null)
            {
                try { await _connection.CloseAsync(cancellationToken: cancellationToken); } catch { }
                _connection.Dispose();
                _connection = null;
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        if (!_started || _disposed) return false;
        if (_connection?.IsOpen != true) return false;

        try
        {
            IChannel test = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            await test.CloseAsync(cancellationToken);
            test.Dispose();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IChannel> GetPublishChannelAsync(CancellationToken cancellationToken = default)
    {
        if (!_started) throw new InvalidOperationException("Connection manager must be started before publishing.");

        await EnsureConnectionAsync(cancellationToken);

        if (_publishChannel != null && _publishChannel.IsOpen)
        {
            return _publishChannel;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            await EnsureConnectionAsync(cancellationToken);

            if (_publishChannel != null && _publishChannel.IsOpen)
            {
                return _publishChannel;
            }

            _publishChannel = await _connection!.CreateChannelAsync(cancellationToken: cancellationToken);
            _publishChannel.BasicReturnAsync += (sender, args) =>
            {
                _logger.Warning($"RabbitMQ returned message (unroutable). ReplyCode={args.ReplyCode} ReplyText={args.ReplyText} Exchange={args.Exchange} RoutingKey={args.RoutingKey}");
                return Task.CompletedTask;
            };

            return _publishChannel;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task StartConsumingAsync(
        string queueName,
        Func<string, Dictionary<string, object?>, Task<bool>> messageHandler,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentException("Queue name cannot be null or empty", nameof(queueName));
        if (messageHandler == null) throw new ArgumentNullException(nameof(messageHandler));
        if (!_started) throw new InvalidOperationException("Connection manager must be started before consuming.");

        await EnsureConnectionAsync(cancellationToken);

        if (_consumerChannels.TryGetValue(queueName, out IChannel? existing) && existing.IsOpen)
        {
            return;
        }

        IChannel channel = await _connection!.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: (ushort)Math.Clamp(_configuration.Site.MaxConcurrentHandlers, 1, 1000),
            global: false,
            cancellationToken: cancellationToken);

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            bool ok = false;
            try
            {
                string body = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                Dictionary<string, object?> headers = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                if (eventArgs.BasicProperties?.Headers != null)
                {
                    foreach (KeyValuePair<string, object?> kvp in eventArgs.BasicProperties.Headers)
                    {
                        headers[kvp.Key] = kvp.Value;
                    }
                }

                ok = await messageHandler(body, headers);
            }
            catch (Exception ex)
            {
                _logger.Error($"Unhandled exception in consumer for {queueName}", ex);
                ok = false;
            }

            try
            {
                if (ok)
                {
                    await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: CancellationToken.None);
                }
                else
                {
                    await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true, cancellationToken: CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to ack/nack message from {queueName}", ex);
            }
        };

        string tag = await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);

        _consumerChannels[queueName] = channel;
        _consumerTags[queueName] = tag;

        _logger.Info($"Started consumer for queue '{queueName}' tag '{tag}'");
    }

    private async Task EnsureConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection?.IsOpen == true) return;

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection?.IsOpen == true) return;

            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = _configuration.Connection.HostName,
                Port = _configuration.Connection.Port,
                UserName = _configuration.Connection.Username,
                Password = _configuration.Connection.Password,
                VirtualHost = _configuration.Connection.VirtualHost,
                RequestedHeartbeat = TimeSpan.FromSeconds(_configuration.Connection.RequestedHeartbeatSeconds),
                AutomaticRecoveryEnabled = _configuration.Connection.AutomaticRecoveryEnabled,
                TopologyRecoveryEnabled = _configuration.Connection.AutomaticRecoveryEnabled
            };

            if (_configuration.Connection.UseTLS)
            {
                factory.Ssl.Enabled = true;
                if (!string.IsNullOrWhiteSpace(_configuration.Connection.TLSServerName))
                {
                    factory.Ssl.ServerName = _configuration.Connection.TLSServerName;
                }
            }

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _connection.ConnectionShutdownAsync += (sender, args) =>
            {
                _logger.Warning($"RabbitMQ connection shutdown: {args.ReplyText}");
                return Task.CompletedTask;
            };
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            StopAsync().GetAwaiter().GetResult();
        }
        catch
        {
        }

        _connectionLock.Dispose();
    }
}
