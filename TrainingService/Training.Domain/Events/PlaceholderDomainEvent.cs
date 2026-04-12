namespace Training.Domain.Events;

public sealed record PlaceholderDomainEvent(Guid EntityId, DateTimeOffset OccurredOn);
