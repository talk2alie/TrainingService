using Training.Application.DTOs;
using Training.Application.Interfaces;
using Training.Domain.Routines;

namespace Training.Application.UseCases.Routines;

public sealed class ListRoutinesUseCase
{
    private readonly IRoutineRepository _routineRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public ListRoutinesUseCase(IRoutineRepository routineRepository, ICurrentUserAccessor currentUserAccessor)
    {
        _routineRepository = routineRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<IReadOnlyList<RoutineSummaryResponse>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var ownerUserId = _currentUserAccessor.GetRequiredUserId();
        var routines = await _routineRepository.ListByOwnerAsync(ownerUserId, cancellationToken);

        return routines.Select(routine => new RoutineSummaryResponse(
            routine.Id,
            routine.Name.Value,
            routine.Description.Value,
            routine.DifficultyLevel.ToString(),
            routine.IsArchived,
            routine.Exercises.Select(x => new RoutineExerciseResponse(x.Id, x.ExerciseId, x.ExerciseName.Value, x.Order.Value, x.PlannedSets.Count)).ToArray())).ToArray();
    }
}
