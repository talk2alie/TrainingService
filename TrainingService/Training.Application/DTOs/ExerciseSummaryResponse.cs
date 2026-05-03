namespace Training.Application.DTOs;

public sealed record ExerciseSummaryResponse(
    Guid Id,
    string Name,
    string Description,
    string DifficultyLevel,
    string Category,
    string MovementPattern,
    string PrimaryMuscleGroup,
    IReadOnlyList<string> SecondaryMuscleGroups,
    IReadOnlyList<string> RequiredEquipment);
