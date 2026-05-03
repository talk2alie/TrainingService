using Training.Application.DTOs;
using Training.Domain.Exercises;

namespace Training.Application.UseCases.Exercises;

public sealed class ListExercisesUseCase
{
    private readonly IExerciseRepository _exerciseRepository;

    public ListExercisesUseCase(IExerciseRepository exerciseRepository)
    {
        _exerciseRepository = exerciseRepository;
    }

    public async Task<IReadOnlyList<ExerciseSummaryResponse>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var exercises = await _exerciseRepository.ListAsync(cancellationToken);
        return exercises.Select(exercise => new ExerciseSummaryResponse(
            exercise.Id,
            exercise.Name.Value,
            exercise.Description.Value,
            exercise.DifficultyLevel.ToString(),
            exercise.Category.ToString(),
            exercise.MovementPattern.ToString(),
            exercise.PrimaryMuscleGroup.Name.Value,
            exercise.SecondaryMuscleGroups.Select(x => x.Name.Value).ToArray(),
            exercise.RequiredEquipment.Select(x => x.ToString()).ToArray())).ToArray();
    }
}
