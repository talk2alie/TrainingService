namespace Training.Domain.Exercises;

public sealed class ExerciseReferenceValidator : IExerciseReferenceValidator
{
    private readonly HashSet<Guid> _existingExerciseIds;

    public ExerciseReferenceValidator(IEnumerable<Guid> existingExerciseIds)
    {
        _existingExerciseIds = [.. existingExerciseIds];
    }

    public void EnsureExerciseExists(Guid exerciseId)
    {
        if (!_existingExerciseIds.Contains(exerciseId))
        {
            throw new ExerciseNotFoundException(exerciseId);
        }
    }

    public void EnsureExercisesExist(IEnumerable<Guid> exerciseIds)
    {
        foreach (var id in exerciseIds)
        {
            EnsureExerciseExists(id);
        }
    }
}
