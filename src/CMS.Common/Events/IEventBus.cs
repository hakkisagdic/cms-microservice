namespace CMS.Common.Events;

public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IIntegrationEvent;
    Task SubscribeAsync<T, THandler>(CancellationToken cancellationToken = default) 
        where T : IIntegrationEvent 
        where THandler : class, IIntegrationEventHandler<T>;
}

public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}

public interface IIntegrationEventHandler<in T> where T : IIntegrationEvent
{
    Task HandleAsync(T @event, CancellationToken cancellationToken = default);
}

public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public string EventType { get; init; } = string.Empty;

    protected IntegrationEvent()
    {
        EventType = GetType().Name;
    }
}

// Domain Events
public record UserCreatedEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName
) : IntegrationEvent;

public record UserUpdatedEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName
) : IntegrationEvent;

public record UserDeletedEvent(Guid UserId) : IntegrationEvent;

public record ContentCreatedEvent(
    Guid ContentId,
    Guid AuthorId,
    string Title,
    string Category
) : IntegrationEvent;

public record ContentUpdatedEvent(
    Guid ContentId,
    Guid AuthorId,  
    string Title,
    string Category
) : IntegrationEvent;

public record ContentDeletedEvent(Guid ContentId, Guid AuthorId) : IntegrationEvent;
