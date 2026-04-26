using Training.Domain.Common;

namespace Training.Domain.Exercises;

public sealed record ExerciseCreated(
    Guid ExerciseId,
    ExerciseName Name,
    DateTimeOffset OccurredAtUtc
) : IDomainEvent;
