namespace Training.Domain.Exercises;

public sealed class ExerciseReferenceValidator : IExerciseReferenceValidator
{
    private readonly HashSet<Guid> _existingExerciseIds;

    public ExerciseReferenceValidator(IEnumerable<Guid> existingExerciseIds)
    {
        _existingExerciseIds = new HashSet<Guid>(existingExerciseIds);
    }

    public void EnsureExerciseExists(Guid exerciseId)
    {
        if (!_existingExerciseIds.Contains(exerciseId))
            throw new InvalidOperationException($"Exercise {exerciseId} does not exist.");
    }
}
