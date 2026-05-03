using Training.Domain.Common;
using Training.Domain.Exercises;
using Training.Domain.Routines;

namespace Training.Domain.Tests;

public sealed class RoutineAggregateTests
{
    [Fact]
    public void Create_WithNoExercises_Succeeds()
    {
        var routine = Routine.Create(
            Guid.NewGuid(),
            RoutineName.Create("Push Day"),
            RoutineDescription.Create("Upper body pressing focus."),
            DifficultyLevel.Intermediate,
            []
        );
        Assert.Empty(routine.Exercises);
    }

    [Fact]
    public void AddExercise_WorksOnEmptyRoutine()
    {
        var routine = Routine.Create(
            Guid.NewGuid(),
            RoutineName.Create("Push Day"),
            RoutineDescription.Create("Upper body pressing focus."),
            DifficultyLevel.Intermediate,
            new List<RoutineExercise>()
        );
        var exerciseId = Guid.NewGuid();
        routine.AddExercise(exerciseId, ExerciseName.Create("Bench Press"), new List<PlannedSet> { CreatePlannedSet(1) });
        Assert.Single(routine.Exercises);
        Assert.Equal(1, routine.Exercises[0].Order.Value);
    }

    [Fact]
    public void RemoveExercise_WhenLastExercise_ThrowsInvalidOperationException()
    {
        var routine = Routine.Create(
            Guid.NewGuid(),
            RoutineName.Create("Push Day"),
            RoutineDescription.Create("Upper body pressing focus."),
            DifficultyLevel.Intermediate,
            []
        );
        var exerciseId = routine.AddExercise(Guid.NewGuid(), ExerciseName.Create("Bench Press"), new List<PlannedSet> { CreatePlannedSet(1) });
        var ex = Assert.Throws<InvalidRoutineOperationException>(() => routine.RemoveExercise(routine.Exercises[0].Id));
        Assert.Equal("A routine must contain at least one exercise.", ex.Message);
    }

    [Fact]
    public void Archive_PreventsArchivingEmptyRoutine()
    {
        var routine = Routine.Create(
            Guid.NewGuid(),
            RoutineName.Create("Push Day"),
            RoutineDescription.Create("Upper body pressing focus."),
            DifficultyLevel.Intermediate,
            []
        );
        var ex = Assert.Throws<InvalidRoutineOperationException>(() => routine.Archive());
        Assert.Equal("Cannot archive a routine with no exercises.", ex.Message);
    }

    [Fact]
    public void InvariantMethods_DoNotThrow_OnEmptyRoutine()
    {
        var routine = Routine.Create(
            Guid.NewGuid(),
            RoutineName.Create("Push Day"),
            RoutineDescription.Create("Upper body pressing focus."),
            DifficultyLevel.Intermediate,
            []
        );
        // Should not throw
        routine.GetType().GetMethod("EnsureRoutineExerciseIdsAreUnique", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(routine, [new List<RoutineExercise>()]);
        routine.GetType().GetMethod("EnsureRoutineExerciseInvariants", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(routine, [new List<RoutineExercise>()]);
        routine.GetType().GetMethod("EnsureNoDuplicateExercises", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(routine, [new List<RoutineExercise>()]);
        routine.GetType().GetMethod("EnsureUniqueExerciseOrder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(routine, [new List<RoutineExercise>()]);
        routine.GetType().GetMethod("EnsureSequentialExerciseOrder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(routine, [new List<RoutineExercise>()]);
    }

    // Existing tests updated to always add at least one exercise before performing operations
    [Fact]
    public void MoveExercise_ReordersAndNormalizesOrder()
    {
        var routine = Routine.Create(
            Guid.NewGuid(),
            RoutineName.Create("Push Day"),
            RoutineDescription.Create("Upper body pressing focus."),
            DifficultyLevel.Intermediate,
            new List<RoutineExercise>()
        );
        var firstRoutineExerciseId = routine.AddExercise(Guid.NewGuid(), ExerciseName.Create("Bench Press"), new List<PlannedSet> { CreatePlannedSet(1) });
        var secondRoutineExerciseId = routine.AddExercise(Guid.NewGuid(), ExerciseName.Create("Incline Dumbbell Press"), new List<PlannedSet> { CreatePlannedSet(1) });
        routine.MoveExercise(secondRoutineExerciseId, Order.Create(1));
        Assert.Equal(secondRoutineExerciseId, routine.Exercises[0].Id);
        Assert.Equal(1, routine.Exercises[0].Order.Value);
        Assert.Equal(2, routine.Exercises[1].Order.Value);
    }

    [Fact]
    public void AddPlannedSet_WithNonSequentialOrder_ThrowsInvalidOperationException()
    {
        var routine = Routine.Create(
            Guid.NewGuid(),
            RoutineName.Create("Push Day"),
            RoutineDescription.Create("Upper body pressing focus."),
            DifficultyLevel.Intermediate,
            new List<RoutineExercise>()
        );
        var exerciseId = routine.AddExercise(Guid.NewGuid(), ExerciseName.Create("Bench Press"), new List<PlannedSet> { CreatePlannedSet(1) });
        var routineExerciseId = routine.Exercises[0].Id;
        var ex = Assert.Throws<InvalidOperationException>(() => routine.AddPlannedSet(routineExerciseId, CreatePlannedSet(3)));
        Assert.Equal("Planned set order must be sequential. Expected order 2.", ex.Message);
    }

    [Fact]
    public void RemovePlannedSet_WhenOnlySetExists_ThrowsInvalidOperationException()
    {
        var routine = Routine.Create(
            Guid.NewGuid(),
            RoutineName.Create("Push Day"),
            RoutineDescription.Create("Upper body pressing focus."),
            DifficultyLevel.Intermediate,
            new List<RoutineExercise>()
        );
        var exerciseId = routine.AddExercise(Guid.NewGuid(), ExerciseName.Create("Bench Press"), new List<PlannedSet> { CreatePlannedSet(1) });
        var routineExerciseId = routine.Exercises[0].Id;
        var ex = Assert.Throws<InvalidOperationException>(() => routine.RemovePlannedSet(routineExerciseId, Order.Create(1)));
        Assert.Equal("A routine exercise must contain at least one planned set.", ex.Message);
    }

    [Fact]
    public void RemovePlannedSet_NormalizesRemainingSetOrder()
    {
        var routine = Routine.Create(
            Guid.NewGuid(),
            RoutineName.Create("Push Day"),
            RoutineDescription.Create("Upper body pressing focus."),
            DifficultyLevel.Intermediate,
            new List<RoutineExercise>()
        );
        var exerciseId = routine.AddExercise(Guid.NewGuid(), ExerciseName.Create("Bench Press"), new List<PlannedSet> { CreatePlannedSet(1) });
        var routineExerciseId = routine.Exercises[0].Id;
        routine.AddPlannedSet(routineExerciseId, CreatePlannedSet(2));
        routine.AddPlannedSet(routineExerciseId, CreatePlannedSet(3));
        routine.RemovePlannedSet(routineExerciseId, Order.Create(2));
        var orders = routine.Exercises[0].PlannedSets.Select(x => x.Order.Value).ToArray();
        Assert.Equal(new[] { 1, 2 }, orders);
    }

    [Fact]
    public void AddExercise_WithDuplicateExerciseId_ThrowsInvalidOperationException()
    {
        var routine = Routine.Create(
            Guid.NewGuid(),
            RoutineName.Create("Push Day"),
            RoutineDescription.Create("Upper body pressing focus."),
            DifficultyLevel.Intermediate,
            new List<RoutineExercise>()
        );
        var exerciseId = routine.AddExercise(Guid.NewGuid(), ExerciseName.Create("Bench Press"), new List<PlannedSet> { CreatePlannedSet(1) });
        var duplicateExerciseId = routine.Exercises[0].ExerciseId;
        var ex = Assert.Throws<RoutineInvariantViolationException>(() => routine.AddExercise(duplicateExerciseId, ExerciseName.Create("Bench Press Variation"), new List<PlannedSet> { CreatePlannedSet(1) }));
        Assert.Equal("Duplicate ExerciseId detected.", ex.Message);
    }

    [Fact]
    public void AddExercise_WithEmptyPlannedSets_ThrowsArgumentException()
    {
        var routine = Routine.Create(
            Guid.NewGuid(),
            RoutineName.Create("Push Day"),
            RoutineDescription.Create("Upper body pressing focus."),
            DifficultyLevel.Intermediate,
            new List<RoutineExercise>()
        );
        var ex = Assert.Throws<ArgumentException>(() => routine.AddExercise(Guid.NewGuid(), ExerciseName.Create("Incline Dumbbell Press"), new List<PlannedSet>()));
        Assert.Equal("plannedSets", ex.ParamName);
    }

    [Fact]
    public void AddExercise_WithNonSequentialPlannedSets_ThrowsArgumentException()
    {
        var routine = Routine.Create(
            Guid.NewGuid(),
            RoutineName.Create("Push Day"),
            RoutineDescription.Create("Upper body pressing focus."),
            DifficultyLevel.Intermediate,
            []
        );
        var ex = Assert.Throws<ArgumentException>(() => routine.AddExercise(Guid.NewGuid(), ExerciseName.Create("Incline Dumbbell Press"), new List<PlannedSet> { CreatePlannedSet(2) }));
        Assert.Equal("plannedSets", ex.ParamName);
    }

    [Fact]
    public void RemovePlannedSet_WithUnknownOrderForExercise_ThrowsInvalidOperationException()
    {
        var routine = Routine.Create(
            Guid.NewGuid(),
            RoutineName.Create("Push Day"),
            RoutineDescription.Create("Upper body pressing focus."),
            DifficultyLevel.Intermediate,
            new List<RoutineExercise>()
        );
        var exerciseId = routine.AddExercise(Guid.NewGuid(), ExerciseName.Create("Bench Press"), new List<PlannedSet> { CreatePlannedSet(1) });
        var routineExerciseId = routine.Exercises[0].Id;
        var ex = Assert.Throws<InvalidRoutineOperationException>(() => routine.RemovePlannedSet(routineExerciseId, Order.Create(2)));
        Assert.Equal("The planned set was not found for the routine exercise.", ex.Message);
    }

    private static PlannedSet CreatePlannedSet(int order)
    {
        return PlannedSet.Create(Order.Create(order), repetitions: 10, weight: Weight.Create(60));
    }
}
