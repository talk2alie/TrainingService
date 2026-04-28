using Training.Domain.Common;

namespace Training.Domain.Routines;

public sealed record RoutineCreated(
    Guid RoutineId,
    RoutineName Name,
    Guid OwnerUserId,
    DateTimeOffset OccurredAtUtc
) : IDomainEvent;
