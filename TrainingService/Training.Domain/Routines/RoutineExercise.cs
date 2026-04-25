using Training.Domain.Common;
using Training.Domain.ValueObjects;

namespace Training.Domain.Routines;

/// <summary>
/// Represents an exercise entry inside a routine aggregate.
/// </summary>
public sealed class RoutineExercise
{
    private readonly List<PlannedSet> _plannedSets;

    private RoutineExercise(Guid id, Guid exerciseId, ExerciseName exerciseName, Order order, IEnumerable<PlannedSet>? plannedSets)
    {
        Id = Guard.AgainstEmptyGuid(id, nameof(id));
        ExerciseId = Guard.AgainstEmptyGuid(exerciseId, nameof(exerciseId));
        ExerciseName = Guard.AgainstNull(exerciseName, nameof(exerciseName));
        Order = Guard.AgainstNull(order, nameof(order));
        _plannedSets = plannedSets?.ToList() ?? [];

        EnsureUniqueSetOrder(_plannedSets);
    }

    /// <summary>
    /// Gets the entity identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the referenced exercise identifier.
    /// </summary>
    public Guid ExerciseId { get; }

    /// <summary>
    /// Gets the exercise name snapshot.
    /// </summary>
    public ExerciseName ExerciseName { get; private set; }

    /// <summary>
    /// Gets the order inside the routine.
    /// </summary>
    public Order Order { get; private set; }

    /// <summary>
    /// Gets the planned sets for the exercise.
    /// </summary>
    public IReadOnlyList<PlannedSet> PlannedSets => _plannedSets.AsReadOnly();

    /// <summary>
    /// Creates a new <see cref="RoutineExercise"/>.
    /// </summary>
    internal static RoutineExercise Create(Guid exerciseId, ExerciseName exerciseName, Order order, IEnumerable<PlannedSet>? plannedSets = null)
    {
        return new RoutineExercise(Guid.NewGuid(), exerciseId, exerciseName, order, plannedSets);
    }

    /// <summary>
    /// Updates the exercise order in the routine.
    /// </summary>
    internal void UpdateOrder(Order order)
    {
        Order = Guard.AgainstNull(order, nameof(order));
    }

    /// <summary>
    /// Renames the exercise snapshot.
    /// </summary>
    internal void RenameExercise(ExerciseName exerciseName)
    {
        ExerciseName = Guard.AgainstNull(exerciseName, nameof(exerciseName));
    }

    /// <summary>
    /// Adds a planned set.
    /// </summary>
    internal void AddPlannedSet(PlannedSet plannedSet)
    {
        Guard.AgainstNull(plannedSet, nameof(plannedSet));

        if (_plannedSets.Any(x => x.Order == plannedSet.Order))
        {
            throw new InvalidOperationException($"A planned set with order {plannedSet.Order.Value} already exists.");
        }

        _plannedSets.Add(plannedSet);
    }

    /// <summary>
    /// Removes a planned set by order.
    /// </summary>
    internal void RemovePlannedSet(Order order)
    {
        Guard.AgainstNull(order, nameof(order));

        var plannedSet = _plannedSets.SingleOrDefault(x => x.Order == order)
            ?? throw new InvalidOperationException($"A planned set with order {order.Value} was not found.");

        _plannedSets.Remove(plannedSet);
    }

    private static void EnsureUniqueSetOrder(IReadOnlyCollection<PlannedSet> plannedSets)
    {
        if (plannedSets.Count != plannedSets.Select(x => x.Order).Distinct().Count())
        {
            throw new ArgumentException("Planned set order values must be unique.", nameof(plannedSets));
        }
    }
}
