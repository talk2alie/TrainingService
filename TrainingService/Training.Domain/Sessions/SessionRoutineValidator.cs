using Training.Domain.Routines;

namespace Training.Domain.Sessions;

public sealed class SessionRoutineValidator : ISessionRoutineValidator
{
    /// <summary>
    /// Validates that a newly created Session is correctly initialized from a Routine.
    /// This validator is ONLY intended to run at session creation time.
    /// After creation, sessions may diverge freely from the routine.
    /// </summary>
    public void ValidateSessionAgainstRoutine(Session session, Routine routine)
    {
        // 1. Ensure every RoutineExercise exists in the Session
        foreach (var routineExercise in routine.Exercises)
        {
            if (!session.Exercises.Any(x => x.ExerciseId == routineExercise.ExerciseId))
            {
                throw new InvalidOperationException(
                    $"Routine exercise {routineExercise.ExerciseId} is missing from the session at initialization.");
            }
        }

        // 2. Ensure the Session does not contain exercises that are not in the Routine
        // (Only at initialization — after that, divergence is allowed)
        foreach (var sessionExercise in session.Exercises)
        {
            if (!routine.Exercises.Any(x => x.ExerciseId == sessionExercise.ExerciseId))
            {
                throw new InvalidOperationException(
                    $"Session exercise {sessionExercise.ExerciseId} does not exist in the routine at initialization.");
            }
        }

        // 3. Ensure no logged sets exist at initialization
        foreach (var sessionExercise in session.Exercises)
        {
            if (sessionExercise.LoggedSets.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Session exercise {sessionExercise.ExerciseId} should not have logged sets at initialization.");
            }
        }
    }
}
