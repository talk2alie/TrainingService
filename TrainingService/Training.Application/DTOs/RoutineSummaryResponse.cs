namespace Training.Application.DTOs;

public sealed record RoutineSummaryResponse(
    Guid Id,
    string Name,
    string Description,
    string DifficultyLevel,
    bool IsArchived,
    IReadOnlyList<RoutineExerciseResponse> Exercises);
