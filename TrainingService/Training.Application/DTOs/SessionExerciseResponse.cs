namespace Training.Application.DTOs;

public sealed record SessionExerciseResponse(
    Guid Id,
    Guid ExerciseId,
    string ExerciseName,
    int Order,
    int LoggedSetCount);
