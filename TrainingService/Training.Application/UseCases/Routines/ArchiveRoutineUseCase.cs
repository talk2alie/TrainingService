using Training.Application.Interfaces;
using Training.Domain.Routines;

namespace Training.Application.UseCases.Routines;

public sealed class ArchiveRoutineUseCase
{
    private readonly IRoutineRepository _routineRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public ArchiveRoutineUseCase(IRoutineRepository routineRepository, ICurrentUserAccessor currentUserAccessor)
    {
        _routineRepository = routineRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var routine = await _routineRepository.GetByIdAsync(id, cancellationToken);
        if (routine is null)
        {
            return false;
        }

        if (routine.OwnerUserId != _currentUserAccessor.GetRequiredUserId())
        {
            return false;
        }

        routine.Archive();
        await _routineRepository.UpdateAsync(routine, cancellationToken);
        return true;
    }
}
