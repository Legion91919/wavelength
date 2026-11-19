namespace Wavelength.SharedKernel;

/// <summary>Raised by Playback exactly once per client play ID, when the play reaches the royalty threshold.</summary>
public sealed record TrackPlayQualified(Guid PlayId, string TrackId, string LabelId, DateTimeOffset QualifiedAt);

/// <summary>Publishes an event to every consumer in other modules. In-process now; a transactional outbox in slice 2.</summary>
public interface IIntegrationEventPublisher
{
    void Publish<TEvent>(TEvent integrationEvent)
        where TEvent : class;
}

/// <summary>Consumers must be idempotent: the same event may arrive more than once.</summary>
public interface IIntegrationEventConsumer<in TEvent>
    where TEvent : class
{
    void Handle(TEvent integrationEvent);
}
