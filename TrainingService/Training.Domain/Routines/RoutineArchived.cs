using Training.Domain.Common;

namespace Training.Domain.Routines;

public sealed record RoutineArchived(
    Guid RoutineId,
    DateTimeOffset OccurredAtUtc
) : IDomainEvent;
