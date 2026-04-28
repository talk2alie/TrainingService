using Training.Domain.Exercises;

namespace Training.Domain.Tests.Exercises;

public class ExerciseReferenceValidatorTests
{
    [Fact]
    public void EnsureExerciseExists_Existing_DoesNotThrow()
    {
        var existingId = Guid.NewGuid();
        var validator = new ExerciseReferenceValidator(new[] { existingId });
        validator.EnsureExerciseExists(existingId);
    }

    [Fact]
    public void EnsureExerciseExists_NotExisting_Throws()
    {
        var validator = new ExerciseReferenceValidator(new[] { Guid.NewGuid() });
        Assert.Throws<InvalidOperationException>(() => validator.EnsureExerciseExists(Guid.NewGuid()));
    }
}
