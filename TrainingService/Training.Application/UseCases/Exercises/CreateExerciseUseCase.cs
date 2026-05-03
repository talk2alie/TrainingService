using Training.Application.DTOs;
using Training.Domain.Exercises;

namespace Training.Application.UseCases.Exercises;

public sealed class CreateExerciseUseCase
{
    private readonly IExerciseRepository _exerciseRepository;

    public CreateExerciseUseCase(IExerciseRepository exerciseRepository)
    {
        _exerciseRepository = exerciseRepository;
    }

    public async Task<ExerciseSummaryResponse> ExecuteAsync(
        string name,
        string description,
        DifficultyLevel difficultyLevel,
        ExerciseCategory category,
        MovementPattern movementPattern,
        MuscleGroup primaryMuscleGroup,
        IEnumerable<EquipmentType> requiredEquipment,
        CancellationToken cancellationToken)
    {
        var exercise = Exercise.Create(
            ExerciseName.Create(name),
            ExerciseDescription.Create(description),
            difficultyLevel,
            category,
            movementPattern,
            primaryMuscleGroup,
            Hyperlink.Create("https://example.com/instruction"),
            ImageData.Create([1], ImageFormat.Png),
            requiredEquipment);

        await _exerciseRepository.AddAsync(exercise, cancellationToken);

        return new ExerciseSummaryResponse(
            exercise.Id,
            exercise.Name.Value,
            exercise.Description.Value,
            exercise.DifficultyLevel.ToString(),
            exercise.Category.ToString(),
            exercise.MovementPattern.ToString(),
            exercise.PrimaryMuscleGroup.Name.Value,
            exercise.SecondaryMuscleGroups.Select(x => x.Name.Value).ToArray(),
            exercise.RequiredEquipment.Select(x => x.ToString()).ToArray());
    }
}
