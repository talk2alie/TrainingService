namespace Training.Domain.Exercises;

public interface IExerciseReferenceValidator
{
    void EnsureExerciseExists(Guid exerciseId);
}
