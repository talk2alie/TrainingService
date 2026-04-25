using Training.Domain.Common;
using Training.Domain.ValueObjects;

namespace Training.Domain.Sessions;

/// <summary>
/// Represents a performed training session aggregate root.
/// </summary>
public sealed class Session
{
    private readonly List<SessionExercise> _exercises;

    private Session(
        Guid id,
        Guid? routineId,
        DateTimeOffset startedAtUtc,
        IEnumerable<SessionExercise>? exercises,
        SessionNote? note)
    {
        Id = Guard.AgainstEmptyGuid(id, nameof(id));

        if (routineId.HasValue)
        {
            Guard.AgainstEmptyGuid(routineId.Value, nameof(routineId));
        }

        if (startedAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Session start time must be in UTC.", nameof(startedAtUtc));
        }

        RoutineId = routineId;
        StartedAtUtc = startedAtUtc;
        Note = note;
        _exercises = exercises?.ToList() ?? [];

        EnsureUniqueExerciseOrder(_exercises);
    }

    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the optional source routine identifier.
    /// </summary>
    public Guid? RoutineId { get; }

    /// <summary>
    /// Gets the session start time in UTC.
    /// </summary>
    public DateTimeOffset StartedAtUtc { get; }

    /// <summary>
    /// Gets the session completion time in UTC, when completed.
    /// </summary>
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the session is completed.
    /// </summary>
    public bool IsCompleted => CompletedAtUtc.HasValue;

    /// <summary>
    /// Gets the optional session note.
    /// </summary>
    public SessionNote? Note { get; private set; }

    /// <summary>
    /// Gets the exercises performed in the session.
    /// </summary>
    public IReadOnlyList<SessionExercise> Exercises => _exercises.AsReadOnly();

    /// <summary>
    /// Creates a new <see cref="Session"/>.
    /// </summary>
    public static Session Start(Guid? routineId = null, IEnumerable<SessionExercise>? exercises = null, SessionNote? note = null)
    {
        return new Session(Guid.NewGuid(), routineId, DateTimeOffset.UtcNow, exercises, note);
    }

    /// <summary>
    /// Updates the session note.
    /// </summary>
    public void UpdateNote(SessionNote note)
    {
        Note = Guard.AgainstNull(note, nameof(note));
    }

    /// <summary>
    /// Clears the session note.
    /// </summary>
    public void ClearNote()
    {
        Note = null;
    }

    /// <summary>
    /// Adds an exercise to the session.
    /// </summary>
    public Guid AddExercise(Guid exerciseId, ExerciseName exerciseName, IEnumerable<LoggedSet>? loggedSets = null)
    {
        EnsureNotCompleted();

        Guard.AgainstEmptyGuid(exerciseId, nameof(exerciseId));
        Guard.AgainstNull(exerciseName, nameof(exerciseName));

        if (_exercises.Any(x => x.ExerciseId == exerciseId))
        {
            throw new InvalidOperationException("The session already contains this exercise.");
        }

        var order = Order.Create(_exercises.Count + 1);
        var exercise = SessionExercise.Create(exerciseId, exerciseName, order, loggedSets);
        _exercises.Add(exercise);

        return exercise.Id;
    }

    /// <summary>
    /// Adds a logged set to an exercise in the session.
    /// </summary>
    public void AddLoggedSet(Guid sessionExerciseId, LoggedSet loggedSet)
    {
        EnsureNotCompleted();

        Guard.AgainstEmptyGuid(sessionExerciseId, nameof(sessionExerciseId));
        Guard.AgainstNull(loggedSet, nameof(loggedSet));

        var exercise = _exercises.SingleOrDefault(x => x.Id == sessionExerciseId)
            ?? throw new InvalidOperationException("The session exercise was not found.");

        exercise.AddLoggedSet(loggedSet);
    }

    /// <summary>
    /// Renames an exercise snapshot in the session.
    /// </summary>
    public void RenameExercise(Guid sessionExerciseId, ExerciseName exerciseName)
    {
        EnsureNotCompleted();

        Guard.AgainstEmptyGuid(sessionExerciseId, nameof(sessionExerciseId));
        Guard.AgainstNull(exerciseName, nameof(exerciseName));

        var exercise = _exercises.SingleOrDefault(x => x.Id == sessionExerciseId)
            ?? throw new InvalidOperationException("The session exercise was not found.");

        exercise.RenameExercise(exerciseName);
    }

    /// <summary>
    /// Removes a logged set from an exercise in the session by set order.
    /// </summary>
    public void RemoveLoggedSet(Guid sessionExerciseId, Order setOrder)
    {
        EnsureNotCompleted();

        Guard.AgainstEmptyGuid(sessionExerciseId, nameof(sessionExerciseId));
        Guard.AgainstNull(setOrder, nameof(setOrder));

        var exercise = _exercises.SingleOrDefault(x => x.Id == sessionExerciseId)
            ?? throw new InvalidOperationException("The session exercise was not found.");

        exercise.RemoveLoggedSet(setOrder);
    }

    /// <summary>
    /// Marks the session as completed.
    /// </summary>
    public void Complete(DateTimeOffset completedAtUtc)
    {
        EnsureNotCompleted();

        if (completedAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Session completion time must be in UTC.", nameof(completedAtUtc));
        }

        if (completedAtUtc < StartedAtUtc)
        {
            throw new InvalidOperationException("Completion time cannot be earlier than start time.");
        }

        CompletedAtUtc = completedAtUtc;
    }

    private void EnsureNotCompleted()
    {
        if (IsCompleted)
        {
            throw new InvalidOperationException("Cannot modify a completed session.");
        }
    }

    private static void EnsureUniqueExerciseOrder(IReadOnlyCollection<SessionExercise> exercises)
    {
        if (exercises.Count != exercises.Select(x => x.Order).Distinct().Count())
        {
            throw new ArgumentException("Session exercise order values must be unique.", nameof(exercises));
        }
    }
}
