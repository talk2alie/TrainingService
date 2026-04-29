namespace Training.Domain.Exercises;

public sealed class ExerciseNotFoundException : Exception
{
    public ExerciseNotFoundException(Guid exerciseId)
        : base($"Exercise {exerciseId} does not exist.")
    {
        ExerciseId = exerciseId;
    }

    public Guid ExerciseId { get; }
}
