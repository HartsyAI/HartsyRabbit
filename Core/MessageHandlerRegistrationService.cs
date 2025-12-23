using Microsoft.Extensions.DependencyInjection;

namespace HartsyRabbit.Core;

public class MessageHandlerRegistrationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Dictionary<Type, IServiceScope> _handlerScopes = new();
    private readonly List<MessageHandlerRegistration> _registrations;
    private readonly object _lock = new();

    public MessageHandlerRegistrationService(IServiceProvider serviceProvider, IEnumerable<MessageHandlerRegistration> registrations)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _scopeFactory = _serviceProvider.GetRequiredService<IServiceScopeFactory>();
        _registrations = registrations?.ToList() ?? new List<MessageHandlerRegistration>();
    }

    public IReadOnlyList<MessageHandlerRegistration> GetRegistrations()
    {
        return _registrations.AsReadOnly();
    }

    public IEnumerable<MessageHandlerRegistration> GetHandlersForMessageType(Type messageType)
    {
        return _registrations.Where(r => r.MessageType == messageType).OrderByDescending(r => r.Priority).ToList();
    }

    public IEnumerable<MessageHandlerRegistration> GetHandlersForMessageType(string messageTypeName)
    {
        if (string.IsNullOrWhiteSpace(messageTypeName))
        {
            return Array.Empty<MessageHandlerRegistration>();
        }

        return _registrations.Where(r => r.MessageType.Name.Equals(messageTypeName, StringComparison.Ordinal)).OrderByDescending(r => r.Priority).ToList();
    }

    public ITypeSafeMessageHandler<TMessage> CreateHandler<TMessage>(MessageHandlerRegistration registration) where TMessage : class
    {
        if (registration == null)
        {
            throw new ArgumentNullException(nameof(registration));
        }

        IServiceProvider handlerProvider = GetHandlerServiceProvider(registration.HandlerType);
        object handler = handlerProvider.GetRequiredService(registration.HandlerType);

        if (handler is not ITypeSafeMessageHandler<TMessage> typedHandler)
        {
            throw new InvalidOperationException($"Handler {registration.HandlerType.Name} does not implement ITypeSafeMessageHandler<{typeof(TMessage).Name}>");
        }

        return typedHandler;
    }

    public object CreateHandler(MessageHandlerRegistration registration)
    {
        if (registration == null)
        {
            throw new ArgumentNullException(nameof(registration));
        }

        IServiceProvider handlerProvider = GetHandlerServiceProvider(registration.HandlerType);
        return handlerProvider.GetRequiredService(registration.HandlerType);
    }

    private IServiceProvider GetHandlerServiceProvider(Type handlerType)
    {
        lock (_lock)
        {
            if (_handlerScopes.TryGetValue(handlerType, out IServiceScope? existing))
            {
                return existing.ServiceProvider;
            }

            IServiceScope scope = _scopeFactory.CreateScope();
            _handlerScopes[handlerType] = scope;
            return scope.ServiceProvider;
        }
    }
}
