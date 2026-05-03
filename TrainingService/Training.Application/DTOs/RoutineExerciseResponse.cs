namespace Training.Application.DTOs;

public sealed record RoutineExerciseResponse(
    Guid Id,
    Guid ExerciseId,
    string ExerciseName,
    int Order,
    int PlannedSetCount);
