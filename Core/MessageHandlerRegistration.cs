using Microsoft.Extensions.DependencyInjection;

namespace HartsyRabbit.Core;

public class MessageHandlerRegistration
{
    public Type MessageType { get; set; } = typeof(object);
    public Type HandlerType { get; set; } = typeof(object);
    public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Scoped;
    public int Priority { get; set; } = 0;
}
