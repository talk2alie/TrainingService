using Training.Domain.Routines;

namespace Training.Domain.Sessions;

public interface ISessionRoutineValidator
{
    void ValidateSessionAgainstRoutine(Session session, Routine routine);
}
