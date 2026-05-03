using Training.Application.DTOs;
using Training.Application.Interfaces;
using Training.Domain.Routines;
using Training.Domain.Exercises;

namespace Training.Application.UseCases.Routines;

public sealed class CreateRoutineUseCase
{
    private readonly IRoutineRepository _routineRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public CreateRoutineUseCase(IRoutineRepository routineRepository, ICurrentUserAccessor currentUserAccessor)
    {
        _routineRepository = routineRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<RoutineSummaryResponse> ExecuteAsync(string name, string description, DifficultyLevel difficultyLevel, CancellationToken cancellationToken)
    {
        var ownerUserId = _currentUserAccessor.GetRequiredUserId();
        var routine = Routine.Create(
            ownerUserId,
            RoutineName.Create(name),
            RoutineDescription.Create(description),
            difficultyLevel,
            []);

        await _routineRepository.AddAsync(routine, cancellationToken);

        return new RoutineSummaryResponse(
            routine.Id,
            routine.Name.Value,
            routine.Description.Value,
            routine.DifficultyLevel.ToString(),
            routine.IsArchived,
            []);
    }
}
