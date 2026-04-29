
using Training.Domain.Routines;
using Training.Domain.Sessions;
using Training.Domain.Exercises;

namespace Training.Domain.Common;

public sealed class AggregatePersistenceValidator : IAggregatePersistenceValidator
{
    public void EnsureRoutineIsPersistable(Routine routine)
    {
        if (routine.Exercises.Count == 0)
        {
            throw new AggregateNotPersistableException("Routine must have at least one exercise before persistence.");
        }
    }

    public void EnsureSessionIsPersistable(Session session)
    {
        if (session.Exercises.Count == 0)
        {
            throw new AggregateNotPersistableException("Session must have at least one exercise before persistence.");
        }
    }

}
