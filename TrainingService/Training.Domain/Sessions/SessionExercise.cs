using Training.Domain.Common;
using Training.Domain.Exercises;

namespace Training.Domain.Sessions;

/// <summary>
/// Represents an exercise entry inside a session aggregate.
/// </summary>
public sealed class SessionExercise
{
    private readonly List<LoggedSet> _loggedSets;

    private SessionExercise(Guid id, Guid exerciseId, ExerciseName exerciseNameSnapshot, Order order, IEnumerable<LoggedSet> loggedSets)
    {
        Id = Guard.AgainstEmptyGuid(id, nameof(id));
        ExerciseId = Guard.AgainstEmptyGuid(exerciseId, nameof(exerciseId));
        ExerciseNameSnapshot = Guard.AgainstNull(exerciseNameSnapshot, nameof(exerciseNameSnapshot));
        Order = Guard.AgainstNull(order, nameof(order));
        ArgumentNullException.ThrowIfNull(loggedSets);
        _loggedSets = loggedSets.ToList();

        if (_loggedSets.Count == 0)
        {
            throw new ArgumentException("A session exercise must contain at least one logged set.", nameof(loggedSets));
        }

        EnsureSequentialSetOrder(_loggedSets);
        EnsureUniqueSetOrder(_loggedSets);
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
    public ExerciseName ExerciseNameSnapshot { get; private set; }

    /// <summary>
    /// Gets the order inside the session.
    /// </summary>
    public Order Order { get; private set; }

    /// <summary>
    /// Gets the logged sets.
    /// </summary>
    public IReadOnlyList<LoggedSet> LoggedSets => _loggedSets.AsReadOnly();

    /// <summary>
    /// Creates a new <see cref="SessionExercise"/>.
    /// </summary>
    internal static SessionExercise Create(Guid exerciseId, ExerciseName exerciseNameSnapshot, Order order, IEnumerable<LoggedSet>? loggedSets = null)
    {
        return new SessionExercise(Guid.NewGuid(), exerciseId, exerciseNameSnapshot, order, loggedSets ?? []);
    }

    /// <summary>
    /// Updates the exercise order in the session.
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
    internal void RenameExercise(ExerciseName exerciseNameSnapshot)
    {
        var validatedName = Guard.AgainstNull(exerciseNameSnapshot, nameof(exerciseNameSnapshot));

        if (ExerciseNameSnapshot.Equals(validatedName))
        {
            throw new InvalidOperationException("Session exercise name is already set to the requested value.");
        }

        ExerciseNameSnapshot = validatedName;
    }

    /// <summary>
    /// Adds a logged set.
    /// </summary>
    internal void AddLoggedSet(LoggedSet loggedSet)
    {
        Guard.AgainstNull(loggedSet, nameof(loggedSet));

        if (loggedSet.Order.Value < 1)
        {
            throw new InvalidOperationException("Logged set order must be at least 1.");
        }

        var expectedOrder = _loggedSets.Count + 1;
        if (loggedSet.Order.Value != expectedOrder)
        {
            throw new InvalidOperationException($"Logged set order must be sequential. Expected order {expectedOrder}.");
        }

        if (_loggedSets.Any(x => x.Order.Value == loggedSet.Order.Value))
        {
            throw new InvalidOperationException($"A logged set with order {loggedSet.Order.Value} already exists.");
        }

        _loggedSets.Add(loggedSet);
    }

    /// <summary>
    /// Updates a logged set by order.
    /// </summary>
    internal void UpdateLoggedSet(Order setOrder, LoggedSet loggedSet)
    {
        Guard.AgainstNull(setOrder, nameof(setOrder));
        Guard.AgainstNull(loggedSet, nameof(loggedSet));

        if (setOrder.Value != loggedSet.Order.Value)
        {
            throw new InvalidOperationException("Logged set order cannot be changed during update.");
        }

        var index = _loggedSets.FindIndex(x => x.Order.Value == setOrder.Value);
        if (index < 0)
        {
            throw new InvalidOperationException($"A logged set with order {setOrder.Value} was not found.");
        }

        if (_loggedSets[index].Equals(loggedSet))
        {
            throw new InvalidOperationException("Logged set is already set to the requested values.");
        }

        _loggedSets[index] = loggedSet;
    }

    /// <summary>
    /// Removes a logged set by order.
    /// </summary>
    internal void RemoveLoggedSet(Order order)
    {
        Guard.AgainstNull(order, nameof(order));

        if (_loggedSets.Count == 1)
        {
            throw new InvalidOperationException("A session exercise must contain at least one logged set.");
        }

        var loggedSet = _loggedSets.SingleOrDefault(x => x.Order.Value == order.Value)
            ?? throw new InvalidOperationException($"A logged set with order {order.Value} was not found.");

        _loggedSets.Remove(loggedSet);
        NormalizeSetOrder();
    }

    private void NormalizeSetOrder()
    {
        var ordered = _loggedSets.OrderBy(x => x.Order.Value).ToList();
        _loggedSets.Clear();

        for (var i = 0; i < ordered.Count; i++)
        {
            var set = ordered[i];
            _loggedSets.Add(set.WithOrder(Order.Create(i + 1)));
        }
    }

    private static void EnsureSequentialSetOrder(IReadOnlyCollection<LoggedSet> loggedSets)
    {
        var expectedOrder = 1;
        foreach (var loggedSet in loggedSets.OrderBy(x => x.Order.Value))
        {
            if (loggedSet.Order.Value != expectedOrder)
            {
                throw new ArgumentException("Logged set order must be sequential, starting at 1.", nameof(loggedSets));
            }

            expectedOrder++;
        }
    }

    private static void EnsureUniqueSetOrder(IReadOnlyCollection<LoggedSet> loggedSets)
    {
        if (loggedSets.Count != loggedSets.Select(x => x.Order.Value).Distinct().Count())
        {
            throw new ArgumentException("Logged set order values must be unique.", nameof(loggedSets));
        }
    }
}
