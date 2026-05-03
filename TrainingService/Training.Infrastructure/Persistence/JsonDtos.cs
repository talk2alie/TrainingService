namespace Training.Infrastructure.Persistence;

public sealed record RoutineExerciseJson(Guid Id, Guid ExerciseId, string ExerciseName, int Order, IReadOnlyList<PlannedSetJson> PlannedSets);
public sealed record PlannedSetJson(int Order, int? Repetitions, decimal? WeightKg, int? DurationSeconds, decimal? DistanceMeters, int? TargetRpe);

public sealed record SessionExerciseJson(Guid Id, Guid ExerciseId, string ExerciseName, int Order, IReadOnlyList<LoggedSetJson> LoggedSets);
public sealed record LoggedSetJson(int Order, int Rpe, int? Repetitions, decimal? WeightKg, int? DurationSeconds, decimal? DistanceMeters, int? HeartRateBpm, string? Note);
