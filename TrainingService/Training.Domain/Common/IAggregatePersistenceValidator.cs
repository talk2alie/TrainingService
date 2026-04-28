using Training.Domain.Routines;
using Training.Domain.Sessions;

namespace Training.Domain.Common;

public interface IAggregatePersistenceValidator
{
    void EnsureRoutineIsPersistable(Routine routine);
    void EnsureSessionIsPersistable(Session session);
}
