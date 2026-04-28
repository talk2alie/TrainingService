using Training.Domain.Common;
using Training.Domain.Exercises;
using Training.Domain.Routines;
using Training.Domain.Sessions;

namespace Training.Domain.Tests.Sessions;

public class SessionRoutineValidatorTests
{
    [Fact]
    public void ValidateSessionAgainstRoutine_AllValid_DoesNotThrow()
    {
        var routine = Routine.Create(Guid.NewGuid(), RoutineName.Create("Test"), RoutineDescription.Create("Desc"), DifficultyLevel.Beginner, []);
        var exerciseId = Guid.NewGuid();
        routine.AddExercise(exerciseId, ExerciseName.Create("Bench Press"), [PlannedSet.Create(Order.Create(1), repetitions: 10, weight: Weight.Create(60))]);
        var session = Session.Start(Guid.NewGuid(), routine.Id, DateTimeOffset.UtcNow, []);
        // Add with a dummy logged set to satisfy aggregate invariant
        session.AddExercise(exerciseId, ExerciseName.Create("Bench Press"), [
            LoggedSet.Create(Order.Create(1), RpeScale.Rpe8, repetitions: 10, weight: Weight.Create(60))
        ]);
        // Remove logged sets via reflection to simulate session creation state
        var sessionExercise = session.Exercises[0];
        var loggedSetsField = typeof(SessionExercise).GetField("_loggedSets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        loggedSetsField?.SetValue(sessionExercise, new List<LoggedSet>());
        var validator = new SessionRoutineValidator();
        validator.ValidateSessionAgainstRoutine(session, routine);
    }

    [Fact]
    public void ValidateSessionAgainstRoutine_ExerciseNotInRoutine_Throws()
    {
        var routine = Routine.Create(Guid.NewGuid(), RoutineName.Create("Test"), RoutineDescription.Create("Desc"), DifficultyLevel.Beginner, []);
        var exerciseId = Guid.NewGuid();
        // Routine does NOT contain this exercise
        var session = Session.Start(Guid.NewGuid(), routine.Id, DateTimeOffset.UtcNow, []);
        // Add with a dummy logged set to satisfy aggregate invariant
        session.AddExercise(exerciseId, ExerciseName.Create("Bench Press"), [
            LoggedSet.Create(Order.Create(1), RpeScale.Rpe8, repetitions: 10, weight: Weight.Create(60))
        ]);
        // Remove logged sets via reflection to simulate session creation state
        var sessionExercise = session.Exercises[0];
        var loggedSetsField = typeof(SessionExercise).GetField("_loggedSets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        loggedSetsField?.SetValue(sessionExercise, new List<LoggedSet>());
        var validator = new SessionRoutineValidator();
        Assert.Throws<InvalidOperationException>(() => validator.ValidateSessionAgainstRoutine(session, routine));
    }

    [Fact]
    public void ValidateSessionAgainstRoutine_LoggedSetsAtInitialization_Throws()
    {
        var routine = Routine.Create(Guid.NewGuid(), RoutineName.Create("Test"), RoutineDescription.Create("Desc"), DifficultyLevel.Beginner, []);
        var exerciseId = Guid.NewGuid();
        routine.AddExercise(exerciseId, ExerciseName.Create("Bench Press"), [PlannedSet.Create(Order.Create(1), repetitions: 10, weight: Weight.Create(60))]);
        var session = Session.Start(Guid.NewGuid(), routine.Id, DateTimeOffset.UtcNow, []);
        // Add SessionExercise with logged sets (should not be allowed at initialization)
        session.AddExercise(exerciseId, ExerciseName.Create("Bench Press"), [
            LoggedSet.Create(Order.Create(1), RpeScale.Rpe8, repetitions: 10, weight: Weight.Create(60))
        ]);
        var validator = new SessionRoutineValidator();
        Assert.Throws<InvalidOperationException>(() => validator.ValidateSessionAgainstRoutine(session, routine));
    }
}
