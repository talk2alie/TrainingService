using Training.Application.DTOs;
using Training.Domain.Exercises;

namespace Training.Application.UseCases.Exercises;

public sealed class GetExerciseByIdUseCase
{
    private readonly IExerciseRepository _exerciseRepository;

    public GetExerciseByIdUseCase(IExerciseRepository exerciseRepository)
    {
        _exerciseRepository = exerciseRepository;
    }

    public async Task<ExerciseSummaryResponse?> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var exercise = await _exerciseRepository.GetByIdAsync(id, cancellationToken);
        if (exercise is null)
        {
            return null;
        }

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
