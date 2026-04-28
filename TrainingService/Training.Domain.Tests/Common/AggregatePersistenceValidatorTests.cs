using Training.Domain.Common;
using Training.Domain.Exercises;
using Training.Domain.Routines;
using Training.Domain.Sessions;

namespace Training.Domain.Tests.Common;

public class AggregatePersistenceValidatorTests
{
    [Fact]
    public void EnsureRoutineIsPersistable_WithExercises_DoesNotThrow()
    {
        var routine = Routine.Create(Guid.NewGuid(), RoutineName.Create("Test"), RoutineDescription.Create("Desc"), DifficultyLevel.Beginner, []);
        routine.AddExercise(Guid.NewGuid(), ExerciseName.Create("Bench Press"), [PlannedSet.Create(Order.Create(1), repetitions: 10, weight: Weight.Create(60))]);
        var validator = new AggregatePersistenceValidator();
        validator.EnsureRoutineIsPersistable(routine);
    }

    [Fact]
    public void EnsureRoutineIsPersistable_Empty_Throws()
    {
        var routine = Routine.Create(Guid.NewGuid(), RoutineName.Create("Test"), RoutineDescription.Create("Desc"), DifficultyLevel.Beginner, []);
        var validator = new AggregatePersistenceValidator();
        Assert.Throws<InvalidOperationException>(() => validator.EnsureRoutineIsPersistable(routine));
    }

    [Fact]
    public void EnsureSessionIsPersistable_WithExercises_DoesNotThrow()
    {
        var session = Session.Start(Guid.NewGuid(), null, DateTimeOffset.UtcNow, []);
        session.AddExercise(Guid.NewGuid(), ExerciseName.Create("Bench Press"), [LoggedSet.Create(Order.Create(1), RpeScale.Rpe8, repetitions: 10, weight: Weight.Create(60))]);
        var validator = new AggregatePersistenceValidator();
        validator.EnsureSessionIsPersistable(session);
    }

    [Fact]
    public void EnsureSessionIsPersistable_Empty_Throws()
    {
        var session = Session.Start(Guid.NewGuid(), null, DateTimeOffset.UtcNow, []);
        var validator = new AggregatePersistenceValidator();
        Assert.Throws<InvalidOperationException>(() => validator.EnsureSessionIsPersistable(session));
    }
}
