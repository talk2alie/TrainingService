using Training.Domain.Exercises;

namespace Training.Api.Requests;

public sealed record CreateExerciseRequest(
    string Name,
    string Description,
    DifficultyLevel DifficultyLevel,
    ExerciseCategory Category,
    MovementPattern MovementPattern,
    string PrimaryMuscleGroup,
    IReadOnlyList<EquipmentType> RequiredEquipment);
