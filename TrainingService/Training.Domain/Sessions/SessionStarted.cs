using Training.Domain.Common;

namespace Training.Domain.Sessions;

public sealed record SessionStarted(
    Guid SessionId,
    Guid OwnerUserId,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset OccurredAtUtc
) : IDomainEvent;
