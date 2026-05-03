namespace Training.Application.DTOs;

public sealed record SessionExerciseDetailResponse(
    Guid Id,
    Guid ExerciseId,
    string ExerciseName,
    int Order,
    IReadOnlyList<LoggedSetResponse> LoggedSets);
