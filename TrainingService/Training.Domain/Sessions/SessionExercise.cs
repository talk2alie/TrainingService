using Training.Domain.Common;
using Training.Domain.Exercises;

namespace Training.Domain.Sessions;

/// <summary>
/// Represents an exercise entry inside a session aggregate.
/// </summary>
public sealed class SessionExercise
{
    private readonly List<LoggedSet> _loggedSets;

    private SessionExercise(Guid id, Guid exerciseId, ExerciseName exerciseName, Order order, IEnumerable<LoggedSet>? loggedSets)
    {
        Id = Guard.AgainstEmptyGuid(id, nameof(id));
        ExerciseId = Guard.AgainstEmptyGuid(exerciseId, nameof(exerciseId));
        ExerciseName = Guard.AgainstNull(exerciseName, nameof(exerciseName));
        Order = Guard.AgainstNull(order, nameof(order));
        _loggedSets = loggedSets?.ToList() ?? [];

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
    public ExerciseName ExerciseName { get; private set; }

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
    internal static SessionExercise Create(Guid exerciseId, ExerciseName exerciseName, Order order, IEnumerable<LoggedSet>? loggedSets = null)
    {
        return new SessionExercise(Guid.NewGuid(), exerciseId, exerciseName, order, loggedSets);
    }

    /// <summary>
    /// Updates the exercise order in the session.
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
    /// Adds a logged set.
    /// </summary>
    internal void AddLoggedSet(LoggedSet loggedSet)
    {
        Guard.AgainstNull(loggedSet, nameof(loggedSet));

        if (_loggedSets.Any(x => x.Order == loggedSet.Order))
        {
            throw new InvalidOperationException($"A logged set with order {loggedSet.Order.Value} already exists.");
        }

        _loggedSets.Add(loggedSet);
    }

    /// <summary>
    /// Removes a logged set by order.
    /// </summary>
    internal void RemoveLoggedSet(Order order)
    {
        Guard.AgainstNull(order, nameof(order));

        var loggedSet = _loggedSets.SingleOrDefault(x => x.Order == order)
            ?? throw new InvalidOperationException($"A logged set with order {order.Value} was not found.");

        _loggedSets.Remove(loggedSet);
    }

    private static void EnsureUniqueSetOrder(IReadOnlyCollection<LoggedSet> loggedSets)
    {
        if (loggedSets.Count != loggedSets.Select(x => x.Order).Distinct().Count())
        {
            throw new ArgumentException("Logged set order values must be unique.", nameof(loggedSets));
        }
    }
}
