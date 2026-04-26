using Training.Domain.Common;

namespace Training.Domain.Sessions;

public sealed record SessionEnded(
    Guid SessionId,
    Guid OwnerUserId,
    DateTimeOffset EndedAtUtc,
    DateTimeOffset OccurredAtUtc
) : IDomainEvent;
