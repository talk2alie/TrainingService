using Training.Application.DTOs;
using Training.Domain.Exercises;

namespace Training.Application.UseCases.Exercises;

public sealed class UpdateExerciseUseCase
{
    private readonly IExerciseRepository _exerciseRepository;

    public UpdateExerciseUseCase(IExerciseRepository exerciseRepository)
    {
        _exerciseRepository = exerciseRepository;
    }

    public async Task<ExerciseSummaryResponse?> ExecuteAsync(
        Guid id,
        string name,
        string description,
        DifficultyLevel difficultyLevel,
        ExerciseCategory category,
        MovementPattern movementPattern,
        MuscleGroup primaryMuscleGroup,
        IEnumerable<EquipmentType> requiredEquipment,
        CancellationToken cancellationToken)
    {
        var existing = await _exerciseRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var updated = Exercise.Rehydrate(
            id,
            ExerciseName.Create(name),
            ExerciseDescription.Create(description),
            difficultyLevel,
            category,
            movementPattern,
            primaryMuscleGroup,
            existing.InstructionVideoUrl,
            existing.Thumbnail,
            requiredEquipment,
            secondaryMuscleGroups: existing.SecondaryMuscleGroups);

        await _exerciseRepository.UpdateAsync(updated, cancellationToken);

        return new ExerciseSummaryResponse(
            updated.Id,
            updated.Name.Value,
            updated.Description.Value,
            updated.DifficultyLevel.ToString(),
            updated.Category.ToString(),
            updated.MovementPattern.ToString(),
            updated.PrimaryMuscleGroup.Name.Value,
            updated.SecondaryMuscleGroups.Select(x => x.Name.Value).ToArray(),
            updated.RequiredEquipment.Select(x => x.ToString()).ToArray());
    }
}
