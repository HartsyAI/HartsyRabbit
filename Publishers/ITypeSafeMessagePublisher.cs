using HartsyRabbit.Core;

namespace HartsyRabbit.Publishers;

public interface ITypeSafeMessagePublisher
{
    Task PublishAsync<TMessage>(GenericMessageEnvelope<TMessage> envelope, CancellationToken cancellationToken = default) where TMessage : class;
    Task PublishDirectAsync<TMessage>(GenericMessageEnvelope<TMessage> envelope, string queueName, CancellationToken cancellationToken = default) where TMessage : class;

    /// <summary>Publishes and waits for the broker to confirm it has the message. Throws when the broker refuses it or cannot route it to a queue.</summary>
    Task PublishConfirmedAsync<TMessage>(GenericMessageEnvelope<TMessage> envelope, CancellationToken cancellationToken = default) where TMessage : class;
}
