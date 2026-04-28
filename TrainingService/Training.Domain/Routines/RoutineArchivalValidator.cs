namespace Training.Domain.Routines;

public interface IActiveSessionChecker
{
    bool HasActiveSessions(Guid routineId);
}

public sealed class RoutineArchivalValidator : IRoutineArchivalValidator
{
    private readonly IActiveSessionChecker _activeSessionChecker;

    public RoutineArchivalValidator(IActiveSessionChecker activeSessionChecker)
    {
        _activeSessionChecker = activeSessionChecker;
    }

    public void EnsureRoutineCanBeArchived(Guid routineId)
    {
        if (_activeSessionChecker.HasActiveSessions(routineId))
        {
            throw new InvalidOperationException($"Routine {routineId} cannot be archived because it is referenced by active sessions.");
        }
    }
}
