using Training.Domain.Common;
using Training.Domain.Exercises;
using Training.Domain.Routines;

namespace Training.Domain.Tests.Routines;

public sealed class RoutineRevalidateTests
{
    [Fact]
    public void AddExercise_DuplicateExerciseName_ThrowsRoutineInvariantViolationException()
    {
        var routine = Routine.Create(
            Guid.NewGuid(),
            RoutineName.Create("Push Day"),
            RoutineDescription.Create("Upper"),
            DifficultyLevel.Beginner,
            []);

        routine.AddExercise(Guid.NewGuid(), ExerciseName.Create("Bench Press"), [PlannedSet.Create(Order.Create(1), repetitions: 10, weight: Weight.Create(60))]);

        var ex = Assert.Throws<RoutineInvariantViolationException>(() =>
            routine.AddExercise(Guid.NewGuid(), ExerciseName.Create("Bench Press"), [PlannedSet.Create(Order.Create(1), repetitions: 8, weight: Weight.Create(50))]));

        Assert.Equal("Duplicate ExerciseName detected.", ex.Message);
    }
}
