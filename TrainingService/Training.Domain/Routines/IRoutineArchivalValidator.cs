namespace Training.Domain.Routines;

public interface IRoutineArchivalValidator
{
    void EnsureRoutineCanBeArchived(Guid routineId);
}
