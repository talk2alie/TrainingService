using Training.Application.DTOs;
using Training.Application.Interfaces;
using Training.Domain.Exercises;
using Training.Domain.Routines;

namespace Training.Application.UseCases.Routines;

public sealed class UpdateRoutineUseCase
{
    private readonly IRoutineRepository _routineRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public UpdateRoutineUseCase(IRoutineRepository routineRepository, ICurrentUserAccessor currentUserAccessor)
    {
        _routineRepository = routineRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<RoutineSummaryResponse?> ExecuteAsync(Guid id, string name, string description, DifficultyLevel difficultyLevel, CancellationToken cancellationToken)
    {
        var routine = await _routineRepository.GetByIdAsync(id, cancellationToken);
        if (routine is null)
        {
            return null;
        }

        if (routine.OwnerUserId != _currentUserAccessor.GetRequiredUserId())
        {
            return null;
        }

        routine.Rename(RoutineName.Create(name));
        routine.UpdateDescription(RoutineDescription.Create(description));
        routine.UpdateDifficultyLevel(difficultyLevel);

        await _routineRepository.UpdateAsync(routine, cancellationToken);

        return new RoutineSummaryResponse(
            routine.Id,
            routine.Name.Value,
            routine.Description.Value,
            routine.DifficultyLevel.ToString(),
            routine.IsArchived,
            routine.Exercises.Select(x => new RoutineExerciseResponse(x.Id, x.ExerciseId, x.ExerciseName.Value, x.Order.Value, x.PlannedSets.Count)).ToArray());
    }
}
