using Training.Domain.Common;
using Training.Domain.ValueObjects;

namespace Training.Domain.Routines;

/// <summary>
/// Represents a reusable training routine aggregate root.
/// </summary>
public sealed class Routine
{
    private readonly List<RoutineExercise> _exercises;

    private Routine(Guid id, RoutineName name, IEnumerable<RoutineExercise>? exercises)
    {
        Id = Guard.AgainstEmptyGuid(id, nameof(id));
        Name = Guard.AgainstNull(name, nameof(name));
        _exercises = exercises?.ToList() ?? [];

        EnsureUniqueExerciseOrder(_exercises);
    }

    /// <summary>
    /// Gets the routine identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the routine name.
    /// </summary>
    public RoutineName Name { get; private set; }

    /// <summary>
    /// Gets the routine exercises.
    /// </summary>
    public IReadOnlyList<RoutineExercise> Exercises => _exercises.AsReadOnly();

    /// <summary>
    /// Creates a new <see cref="Routine"/>.
    /// </summary>
    public static Routine Create(RoutineName name, IEnumerable<RoutineExercise>? exercises = null)
    {
        return new Routine(Guid.NewGuid(), name, exercises);
    }

    /// <summary>
    /// Renames the routine.
    /// </summary>
    public void Rename(RoutineName name)
    {
        Name = Guard.AgainstNull(name, nameof(name));
    }

    /// <summary>
    /// Adds a new exercise to the routine.
    /// </summary>
    public Guid AddExercise(Guid exerciseId, ExerciseName exerciseName, IEnumerable<PlannedSet>? plannedSets = null)
    {
        Guard.AgainstEmptyGuid(exerciseId, nameof(exerciseId));
        Guard.AgainstNull(exerciseName, nameof(exerciseName));

        if (_exercises.Any(x => x.ExerciseId == exerciseId))
        {
            throw new InvalidOperationException("The routine already contains this exercise.");
        }

        var nextOrder = Order.Create(_exercises.Count + 1);
        var routineExercise = RoutineExercise.Create(exerciseId, exerciseName, nextOrder, plannedSets);
        _exercises.Add(routineExercise);
        return routineExercise.Id;
    }

    /// <summary>
    /// Removes an exercise from the routine.
    /// </summary>
    public void RemoveExercise(Guid routineExerciseId)
    {
        Guard.AgainstEmptyGuid(routineExerciseId, nameof(routineExerciseId));

        var exercise = _exercises.SingleOrDefault(x => x.Id == routineExerciseId)
            ?? throw new InvalidOperationException("The routine exercise was not found.");

        _exercises.Remove(exercise);
        NormalizeOrders();
    }

    /// <summary>
    /// Reorders a routine exercise.
    /// </summary>
    public void MoveExercise(Guid routineExerciseId, Order targetOrder)
    {
        Guard.AgainstEmptyGuid(routineExerciseId, nameof(routineExerciseId));
        Guard.AgainstNull(targetOrder, nameof(targetOrder));

        if (targetOrder.Value > _exercises.Count)
        {
            throw new InvalidOperationException("Target order exceeds exercise count.");
        }

        var exercise = _exercises.SingleOrDefault(x => x.Id == routineExerciseId)
            ?? throw new InvalidOperationException("The routine exercise was not found.");

        var sorted = _exercises.OrderBy(x => x.Order.Value).ToList();
        sorted.Remove(exercise);
        sorted.Insert(targetOrder.Value - 1, exercise);

        for (var i = 0; i < sorted.Count; i++)
        {
            sorted[i].UpdateOrder(Order.Create(i + 1));
        }

        _exercises.Clear();
        _exercises.AddRange(sorted);
    }

    /// <summary>
    /// Renames an exercise snapshot in the routine.
    /// </summary>
    public void RenameExercise(Guid routineExerciseId, ExerciseName exerciseName)
    {
        Guard.AgainstEmptyGuid(routineExerciseId, nameof(routineExerciseId));
        Guard.AgainstNull(exerciseName, nameof(exerciseName));

        var exercise = _exercises.SingleOrDefault(x => x.Id == routineExerciseId)
            ?? throw new InvalidOperationException("The routine exercise was not found.");

        exercise.RenameExercise(exerciseName);
    }

    /// <summary>
    /// Adds a planned set to a routine exercise.
    /// </summary>
    public void AddPlannedSet(Guid routineExerciseId, PlannedSet plannedSet)
    {
        Guard.AgainstEmptyGuid(routineExerciseId, nameof(routineExerciseId));
        Guard.AgainstNull(plannedSet, nameof(plannedSet));

        var exercise = _exercises.SingleOrDefault(x => x.Id == routineExerciseId)
            ?? throw new InvalidOperationException("The routine exercise was not found.");

        exercise.AddPlannedSet(plannedSet);
    }

    /// <summary>
    /// Removes a planned set from a routine exercise by set order.
    /// </summary>
    public void RemovePlannedSet(Guid routineExerciseId, Order setOrder)
    {
        Guard.AgainstEmptyGuid(routineExerciseId, nameof(routineExerciseId));
        Guard.AgainstNull(setOrder, nameof(setOrder));

        var exercise = _exercises.SingleOrDefault(x => x.Id == routineExerciseId)
            ?? throw new InvalidOperationException("The routine exercise was not found.");

        exercise.RemovePlannedSet(setOrder);
    }

    private void NormalizeOrders()
    {
        var ordered = _exercises.OrderBy(x => x.Order.Value).ToList();

        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].UpdateOrder(Order.Create(i + 1));
        }
    }

    private static void EnsureUniqueExerciseOrder(IReadOnlyCollection<RoutineExercise> exercises)
    {
        if (exercises.Count != exercises.Select(x => x.Order).Distinct().Count())
        {
            throw new ArgumentException("Routine exercise order values must be unique.", nameof(exercises));
        }
    }
}
