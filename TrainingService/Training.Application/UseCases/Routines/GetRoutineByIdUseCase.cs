using Training.Application.DTOs;
using Training.Application.Interfaces;
using Training.Domain.Routines;

namespace Training.Application.UseCases.Routines;

public sealed class GetRoutineByIdUseCase
{
    private readonly IRoutineRepository _routineRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public GetRoutineByIdUseCase(IRoutineRepository routineRepository, ICurrentUserAccessor currentUserAccessor)
    {
        _routineRepository = routineRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<RoutineSummaryResponse?> ExecuteAsync(Guid id, CancellationToken cancellationToken)
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

        return new RoutineSummaryResponse(
            routine.Id,
            routine.Name.Value,
            routine.Description.Value,
            routine.DifficultyLevel.ToString(),
            routine.IsArchived,
            routine.Exercises.Select(x => new RoutineExerciseResponse(x.Id, x.ExerciseId, x.ExerciseName.Value, x.Order.Value, x.PlannedSets.Count)).ToArray());
    }
}
