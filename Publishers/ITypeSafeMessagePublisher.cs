using HartsyRabbit.Core;

namespace HartsyRabbit.Publishers;

public interface ITypeSafeMessagePublisher
{
    Task PublishAsync<TMessage>(GenericMessageEnvelope<TMessage> envelope, CancellationToken cancellationToken = default) where TMessage : class;
    Task PublishDirectAsync<TMessage>(GenericMessageEnvelope<TMessage> envelope, string queueName, CancellationToken cancellationToken = default) where TMessage : class;
}
