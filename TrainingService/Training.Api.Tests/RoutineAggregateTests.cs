using Training.Domain.Exercises;
using Training.Domain.Common;
using Training.Domain.Routines;

namespace Training.Api.Tests;

public sealed class RoutineAggregateTests
{
    [Fact]
    public void Create_WithValidInput_CreatesRoutineWithFirstExercise()
    {
        var routine = CreateRoutine();

        Assert.Equal(RoutineName.Create("Push Day"), routine.Name);
        Assert.Equal(RoutineDescription.Create("Upper body pressing focus."), routine.Description);
        Assert.Equal(DifficultyLevel.Intermediate, routine.DifficultyLevel);
        Assert.False(routine.IsArchived);
        Assert.Single(routine.Exercises);
        Assert.Equal(1, routine.Exercises[0].Order.Value);
        Assert.Single(routine.Exercises[0].PlannedSets);
        Assert.Equal(1, routine.Exercises[0].PlannedSets[0].Order.Value);
    }

    [Fact]
    public void AddExercise_WithDuplicateExerciseId_ThrowsInvalidOperationException()
    {
        var routine = CreateRoutine();
        var duplicateExerciseId = routine.Exercises[0].ExerciseId;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            routine.AddExercise(
                duplicateExerciseId,
                ExerciseName.Create("Bench Press Variation"),
                [CreatePlannedSet(1)]));

        Assert.Equal("The routine already contains this exercise.", ex.Message);
    }

    [Fact]
    public void RemoveExercise_WhenLastExercise_ThrowsInvalidOperationException()
    {
        var routine = CreateRoutine();

        var ex = Assert.Throws<InvalidOperationException>(() => routine.RemoveExercise(routine.Exercises[0].Id));

        Assert.Equal("A routine must contain at least one exercise.", ex.Message);
    }

    [Fact]
    public void MoveExercise_ReordersAndNormalizesOrder()
    {
        var routine = CreateRoutine();
        var secondExerciseId = Guid.NewGuid();

        routine.AddExercise(secondExerciseId, ExerciseName.Create("Incline Dumbbell Press"), [CreatePlannedSet(1)]);

        var secondRoutineExerciseId = routine.Exercises.Single(x => x.ExerciseId == secondExerciseId).Id;
        routine.MoveExercise(secondRoutineExerciseId, Order.Create(1));

        Assert.Equal(secondExerciseId, routine.Exercises[0].ExerciseId);
        Assert.Equal(1, routine.Exercises[0].Order.Value);
        Assert.Equal(2, routine.Exercises[1].Order.Value);
    }

    [Fact]
    public void AddPlannedSet_WithNonSequentialOrder_ThrowsInvalidOperationException()
    {
        var routine = CreateRoutine();
        var routineExerciseId = routine.Exercises[0].Id;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            routine.AddPlannedSet(routineExerciseId, CreatePlannedSet(3)));

        Assert.Equal("Planned set order must be sequential. Expected order 2.", ex.Message);
    }

    [Fact]
    public void RemovePlannedSet_WhenOnlySetExists_ThrowsInvalidOperationException()
    {
        var routine = CreateRoutine();
        var routineExerciseId = routine.Exercises[0].Id;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            routine.RemovePlannedSet(routineExerciseId, Order.Create(1)));

        Assert.Equal("A routine exercise must contain at least one planned set.", ex.Message);
    }

    [Fact]
    public void RemovePlannedSet_NormalizesRemainingSetOrder()
    {
        var routine = CreateRoutine();
        var routineExerciseId = routine.Exercises[0].Id;

        routine.AddPlannedSet(routineExerciseId, CreatePlannedSet(2));
        routine.AddPlannedSet(routineExerciseId, CreatePlannedSet(3));

        routine.RemovePlannedSet(routineExerciseId, Order.Create(2));

        var orders = routine.Exercises[0].PlannedSets.Select(x => x.Order.Value).ToArray();
        Assert.Equal([1, 2], orders);
    }

    [Fact]
    public void Archive_WhenArchived_BlocksFurtherMutations()
    {
        var routine = CreateRoutine();
        routine.Archive(DateTimeOffset.UtcNow);

        var ex = Assert.Throws<InvalidOperationException>(() => routine.Rename(RoutineName.Create("New Name")));

        Assert.Equal("Cannot modify an archived routine.", ex.Message);
    }

    [Fact]
    public void Archive_WithNonUtcTimestamp_ThrowsArgumentException()
    {
        var routine = CreateRoutine();
        var nonUtc = new DateTimeOffset(2026, 1, 1, 9, 0, 0, TimeSpan.FromHours(2));

        var ex = Assert.Throws<ArgumentException>(() => routine.Archive(nonUtc));

        Assert.Equal("archivedAtUtc", ex.ParamName);
    }

    [Fact]
    public void Archive_WhenEarlierThanCreation_ThrowsInvalidOperationException()
    {
        var routine = CreateRoutine();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            routine.Archive(routine.CreatedAtUtc.AddMinutes(-1)));

        Assert.Equal("Routine archive timestamp cannot be earlier than routine creation time.", ex.Message);
    }

    [Fact]
    public void AddExercise_WithEmptyPlannedSets_ThrowsArgumentException()
    {
        var routine = CreateRoutine();

        var ex = Assert.Throws<ArgumentException>(() =>
            routine.AddExercise(Guid.NewGuid(), ExerciseName.Create("Incline Dumbbell Press"), []));

        Assert.Equal("plannedSets", ex.ParamName);
    }

    [Fact]
    public void AddExercise_WithNonSequentialPlannedSets_ThrowsArgumentException()
    {
        var routine = CreateRoutine();

        var ex = Assert.Throws<ArgumentException>(() =>
            routine.AddExercise(
                Guid.NewGuid(),
                ExerciseName.Create("Incline Dumbbell Press"),
                [CreatePlannedSet(2)]));

        Assert.Equal("plannedSets", ex.ParamName);
    }

    [Fact]
    public void RemovePlannedSet_WithUnknownOrderForExercise_ThrowsInvalidOperationException()
    {
        var routine = CreateRoutine();
        var routineExerciseId = routine.Exercises[0].Id;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            routine.RemovePlannedSet(routineExerciseId, Order.Create(2)));

        Assert.Equal("The planned set was not found for the routine exercise.", ex.Message);
    }

    private static Routine CreateRoutine()
    {
        return Routine.Create(
            Guid.NewGuid(),
            RoutineName.Create("Push Day"),
            RoutineDescription.Create("Upper body pressing focus."),
            DifficultyLevel.Intermediate,
            Guid.NewGuid(),
            ExerciseName.Create("Bench Press"),
            [CreatePlannedSet(1)]);
    }

    private static PlannedSet CreatePlannedSet(int order)
    {
        return PlannedSet.Create(Order.Create(order), repetitions: 10, weight: Weight.Create(60));
    }
}
