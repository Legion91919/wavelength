using Wavelength.SharedKernel;

namespace Wavelength.Api;

/// <summary>
/// Delivers integration events synchronously to every consumer registered by the modules.
/// If a consumer throws, the publisher throws, so the producer can undo and the client can retry.
/// Slice 2 replaces this with a transactional outbox and a background dispatcher.
/// </summary>
internal sealed class InProcessEventPublisher(IServiceProvider services) : IIntegrationEventPublisher
{
    public void Publish<TEvent>(TEvent integrationEvent)
        where TEvent : class
    {
        foreach (var consumer in services.GetServices<IIntegrationEventConsumer<TEvent>>())
        {
            consumer.Handle(integrationEvent);
        }
    }
}
