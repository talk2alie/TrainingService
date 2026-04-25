using Training.Domain.Common;
using Training.Domain.Exercises;

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
        ArgumentNullException.ThrowIfNull(plannedSets);
        _plannedSets = [.. plannedSets];

        EnsureAtLeastOnePlannedSet(_plannedSets);
        EnsureUniqueSetOrder(_plannedSets);
        EnsureSequentialSetOrder(_plannedSets);
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
    internal static RoutineExercise Create(Guid exerciseId, ExerciseName exerciseName, Order order, IEnumerable<PlannedSet> plannedSets)
    {
        return new RoutineExercise(Guid.NewGuid(), exerciseId, exerciseName, order, plannedSets);
    }

    /// <summary>
    /// Updates the exercise order in the routine.
    /// </summary>
    internal void UpdateOrder(Order order)
    {
        var validatedOrder = Guard.AgainstNull(order, nameof(order));

        if (Order.Value == validatedOrder.Value)
        {
            return;
        }

        Order = validatedOrder;
    }

    /// <summary>
    /// Renames the exercise snapshot.
    /// </summary>
    internal void RenameExercise(ExerciseName exerciseName)
    {
        var validatedName = Guard.AgainstNull(exerciseName, nameof(exerciseName));

        if (ExerciseName.Equals(validatedName))
        {
            throw new InvalidOperationException("Routine exercise name is already set to the requested value.");
        }

        ExerciseName = validatedName;
    }

    /// <summary>
    /// Adds a planned set.
    /// </summary>
    internal void AddPlannedSet(PlannedSet plannedSet)
    {
        Guard.AgainstNull(plannedSet, nameof(plannedSet));

        var expectedOrder = _plannedSets.Count + 1;
        if (plannedSet.Order.Value != expectedOrder)
        {
            throw new InvalidOperationException($"Planned set order must be sequential. Expected order {expectedOrder}.");
        }

        if (_plannedSets.Any(x => x.Order.Value == plannedSet.Order.Value))
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

        var plannedSet = _plannedSets.SingleOrDefault(x => x.Order.Value == order.Value)
            ?? throw new InvalidOperationException($"A planned set with order {order.Value} was not found.");

        if (_plannedSets.Count == 1)
        {
            throw new InvalidOperationException("A routine exercise must contain at least one planned set.");
        }

        _plannedSets.Remove(plannedSet);
        NormalizeSetOrder();
    }

    private void NormalizeSetOrder()
    {
        var ordered = _plannedSets.OrderBy(x => x.Order.Value).ToList();
        _plannedSets.Clear();

        for (var i = 0; i < ordered.Count; i++)
        {
            var set = ordered[i];
            _plannedSets.Add(set.WithOrder(Order.Create(i + 1)));
        }
    }

    private static void EnsureAtLeastOnePlannedSet(IReadOnlyCollection<PlannedSet> plannedSets)
    {
        if (plannedSets.Count == 0)
        {
            throw new ArgumentException("At least one planned set must be provided.", nameof(plannedSets));
        }
    }

    private static void EnsureSequentialSetOrder(IReadOnlyCollection<PlannedSet> plannedSets)
    {
        var expectedOrder = 1;
        foreach (var plannedSet in plannedSets.OrderBy(x => x.Order.Value))
        {
            if (plannedSet.Order.Value != expectedOrder)
            {
                throw new ArgumentException("Planned set order must be sequential, starting at 1.", nameof(plannedSets));
            }

            expectedOrder++;
        }
    }

    private static void EnsureUniqueSetOrder(IReadOnlyCollection<PlannedSet> plannedSets)
    {
        if (plannedSets.Count != plannedSets.Select(x => x.Order.Value).Distinct().Count())
        {
            throw new ArgumentException("Planned set order values must be unique.", nameof(plannedSets));
        }
    }
}
