using Training.Domain.Common;
using Training.Domain.Exercises;

namespace Training.Domain.Sessions;

/// <summary>
/// Represents a performed training session aggregate root.
/// </summary>
public sealed class Session
{
    private readonly List<SessionExercise> _exercises = null!;

    private Session(
        Guid id,
        Guid ownerUserId,
        Guid? routineId,
        DateTimeOffset startedAtUtc,
        IEnumerable<SessionExercise> exercises,
        Weight? bodyWeight,
        DateTimeOffset? endedAtUtc,
        SessionNote? note)
    {
        Id = Guard.AgainstEmptyGuid(id, nameof(id));
        OwnerUserId = Guard.AgainstEmptyGuid(ownerUserId, nameof(ownerUserId));

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
        BodyWeight = bodyWeight;
        EndedAtUtc = endedAtUtc;
        Note = note;

        ArgumentNullException.ThrowIfNull(exercises);
        _exercises = [.. exercises];

        if (EndedAtUtc.HasValue)
        {
            if (EndedAtUtc.Value.Offset != TimeSpan.Zero)
            {
                throw new ArgumentException("Session end time must be in UTC.", nameof(endedAtUtc));
            }

            if (EndedAtUtc.Value < StartedAtUtc)
            {
                throw new ArgumentException("Session end time cannot be earlier than session start time.", nameof(endedAtUtc));
            }
        }

        EnsureSessionExerciseIdsAreUnique(_exercises);
        EnsureNoDuplicateExercises(_exercises);
        EnsureSessionExerciseInvariants(_exercises);
        EnsureUniqueExerciseOrder(_exercises);
        EnsureSequentialExerciseOrder(_exercises);
        NormalizeOrders();
    }

    private Session() { }

    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the owning user identifier.
    /// </summary>
    public Guid OwnerUserId { get; }

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
    public DateTimeOffset? EndedAtUtc { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the session is completed.
    /// </summary>
    public bool IsEnded => EndedAtUtc.HasValue;

    /// <summary>
    /// Gets the optional body weight captured during this session.
    /// </summary>
    public Weight? BodyWeight { get; private set; }

    /// <summary>
    /// Gets the derived session duration.
    /// </summary>
    public Duration Duration
    {
        get
        {
            var end = EndedAtUtc ?? DateTimeOffset.UtcNow;
            var elapsed = end - StartedAtUtc;
            return Duration.Create(elapsed <= TimeSpan.Zero ? TimeSpan.FromSeconds(1) : elapsed);
        }
    }

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
    public static Session Create(Guid ownerUserId, DateTimeOffset startedAtUtc, Guid? routineId = null)
    {
        return new Session(Guid.NewGuid(), ownerUserId, routineId, startedAtUtc, [], null, null, null);
    }

    /// <summary>
    /// Starts a new <see cref="Session"/> at the current UTC time.
    /// </summary>
    public static Session Start(Guid ownerUserId, Guid? routineId = null)
    {
        return Create(ownerUserId, DateTimeOffset.UtcNow, routineId);
    }

    /// <summary>
    /// Updates the session note.
    /// </summary>
    public void UpdateNote(SessionNote note)
    {
        EnsureNotEnded();
        Note = Guard.AgainstNull(note, nameof(note));
    }

    /// <summary>
    /// Clears the session note.
    /// </summary>
    public void ClearNote()
    {
        EnsureNotEnded();
        Note = null;
    }

    /// <summary>
    /// Adds or updates the session note.
    /// </summary>
    public void AddNote(SessionNote note)
    {
        UpdateNote(note);
    }

    /// <summary>
    /// Captures body weight for the session.
    /// </summary>
    public void UpdateBodyWeight(Weight bodyWeight)
    {
        EnsureNotEnded();
        BodyWeight = Guard.AgainstNull(bodyWeight, nameof(bodyWeight));
    }

    /// <summary>
    /// Adds an exercise to the session.
    /// </summary>
    public Guid AddExercise(Guid exerciseId, ExerciseName exerciseName, IEnumerable<LoggedSet>? loggedSets = null)
    {
        EnsureNotEnded();

        Guard.AgainstEmptyGuid(exerciseId, nameof(exerciseId));
        Guard.AgainstNull(exerciseName, nameof(exerciseName));

        var loggedSetsList = loggedSets?.ToList() ?? [];

        if (loggedSetsList.Count == 0)
        {
            throw new InvalidOperationException("A session exercise must contain at least one logged set.");
        }

        EnsureValidLoggedSets(loggedSetsList, nameof(loggedSets));

        if (_exercises.Any(x => x.ExerciseId == exerciseId))
        {
            throw new InvalidOperationException("The session already contains this exercise.");
        }

        var order = Order.Create(_exercises.Count + 1);
        var exercise = SessionExercise.Create(exerciseId, exerciseName, order, loggedSetsList);
        _exercises.Add(exercise);

        return exercise.Id;
    }

    /// <summary>
    /// Adds a logged set to an exercise in the session.
    /// </summary>
    public void AddLoggedSet(Guid sessionExerciseId, LoggedSet loggedSet)
    {
        EnsureNotEnded();

        Guard.AgainstEmptyGuid(sessionExerciseId, nameof(sessionExerciseId));
        Guard.AgainstNull(loggedSet, nameof(loggedSet));

        var exercise = GetExerciseById(sessionExerciseId);

        if (exercise.LoggedSets.Any(x => x.Order.Value == loggedSet.Order.Value))
        {
            throw new InvalidOperationException($"The session exercise already contains a logged set with order {loggedSet.Order.Value}.");
        }

        exercise.AddLoggedSet(loggedSet);

        EnsureLoggedSetCountWithinRoutinePlan(exercise);
    }

    /// <summary>
    /// Updates a logged set on a session exercise.
    /// </summary>
    public void UpdateLoggedSet(Guid sessionExerciseId, Order setOrder, LoggedSet loggedSet)
    {
        EnsureNotEnded();

        Guard.AgainstEmptyGuid(sessionExerciseId, nameof(sessionExerciseId));
        Guard.AgainstNull(setOrder, nameof(setOrder));
        Guard.AgainstNull(loggedSet, nameof(loggedSet));

        var exercise = GetExerciseById(sessionExerciseId);
        exercise.UpdateLoggedSet(setOrder, loggedSet);
    }

    /// <summary>
    /// Renames an exercise snapshot in the session.
    /// </summary>
    public void RenameExercise(Guid sessionExerciseId, ExerciseName exerciseName)
    {
        EnsureNotEnded();

        Guard.AgainstEmptyGuid(sessionExerciseId, nameof(sessionExerciseId));
        Guard.AgainstNull(exerciseName, nameof(exerciseName));

        var exercise = GetExerciseById(sessionExerciseId);

        exercise.RenameExercise(exerciseName);
    }

    /// <summary>
    /// Removes a logged set from an exercise in the session by set order.
    /// </summary>
    public void RemoveLoggedSet(Guid sessionExerciseId, Order setOrder)
    {
        EnsureNotEnded();

        Guard.AgainstEmptyGuid(sessionExerciseId, nameof(sessionExerciseId));
        Guard.AgainstNull(setOrder, nameof(setOrder));

        var exercise = GetExerciseById(sessionExerciseId);

        if (exercise.LoggedSets.All(x => x.Order.Value != setOrder.Value))
        {
            throw new InvalidOperationException("The logged set was not found for the session exercise.");
        }

        exercise.RemoveLoggedSet(setOrder);
    }

    /// <summary>
    /// Marks the session as completed.
    /// </summary>
    public void EndSession(DateTimeOffset endedAtUtc)
    {
        EnsureNotEnded();

        if (_exercises.Count == 0)
        {
            throw new InvalidOperationException("A session must contain at least one exercise before it can be ended.");
        }

        if (endedAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Session end time must be in UTC.", nameof(endedAtUtc));
        }

        var nowUtc = DateTimeOffset.UtcNow;
        var minAllowedCompletionTimeUtc = nowUtc.AddMinutes(-1);
        var maxAllowedCompletionTimeUtc = nowUtc.AddMinutes(5);

        if (endedAtUtc < minAllowedCompletionTimeUtc || endedAtUtc > maxAllowedCompletionTimeUtc)
        {
            throw new InvalidOperationException("Session end time is outside the allowed window (from 1 minute in the past to 5 minutes in the future).");
        }

        if (endedAtUtc < StartedAtUtc)
        {
            throw new InvalidOperationException("Session end time cannot be earlier than session start time.");
        }

        EndedAtUtc = endedAtUtc;
    }

    private void EnsureNotEnded()
    {
        if (IsEnded)
        {
            throw new InvalidOperationException("Cannot modify a completed session.");
        }
    }

    private void EnsureLoggedSetCountWithinRoutinePlan(SessionExercise exercise)
    {
        if (!RoutineId.HasValue)
        {
            return;
        }

        // TODO: When Routine is available in the domain service layer,
        // validate logged set count against the actual RoutineExercise planned sets.
        const int MaxLoggedSetsPerExerciseWhenRoutineUnknown = 12;
        if (exercise.LoggedSets.Count > MaxLoggedSetsPerExerciseWhenRoutineUnknown)
        {
            throw new InvalidOperationException("Logged set count exceeds the allowed routine plan capacity for this session exercise.");
        }
    }

    private SessionExercise GetExerciseById(Guid sessionExerciseId)
    {
        return _exercises.SingleOrDefault(x => x.Id == sessionExerciseId)
            ?? throw new InvalidOperationException("The session exercise was not found.");
    }

    private void NormalizeOrders()
    {
        var ordered = _exercises.OrderBy(x => x.Order.Value).ToList();

        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].UpdateOrder(Order.Create(i + 1));
        }
    }

    private static void EnsureSessionExerciseIdsAreUnique(IReadOnlyCollection<SessionExercise> exercises)
    {
        if (exercises.Count != exercises.Select(x => x.Id).Distinct().Count())
        {
            throw new ArgumentException("Session contains duplicate session exercise identifiers, which indicates invalid aggregate composition.", nameof(exercises));
        }
    }

    private static void EnsureNoDuplicateExercises(IReadOnlyCollection<SessionExercise> exercises)
    {
        if (exercises.Count != exercises.Select(x => x.ExerciseId).Distinct().Count())
        {
            throw new ArgumentException("Session cannot contain duplicate exercise references (ExerciseId).", nameof(exercises));
        }
    }

    private static void EnsureSessionExerciseInvariants(IReadOnlyCollection<SessionExercise> exercises)
    {
        foreach (var exercise in exercises)
        {
            if (exercise.Order.Value < 1)
            {
                throw new ArgumentException("Each session exercise must have an order greater than or equal to 1.", nameof(exercises));
            }

            EnsureValidLoggedSets(exercise.LoggedSets, nameof(exercises));
        }
    }

    private static void EnsureValidLoggedSets(IReadOnlyCollection<LoggedSet> loggedSets, string paramName)
    {
        if (loggedSets.Count != loggedSets.Select(x => x.Order.Value).Distinct().Count())
        {
            throw new ArgumentException("Logged set order values must be unique.", paramName);
        }

        var expectedOrder = 1;
        foreach (var loggedSet in loggedSets.OrderBy(x => x.Order.Value))
        {
            if (loggedSet.Order.Value != expectedOrder)
            {
                throw new ArgumentException("Logged set order must be sequential, starting at 1.", paramName);
            }

            expectedOrder++;
        }
    }

    private static void EnsureSequentialExerciseOrder(IReadOnlyCollection<SessionExercise> exercises)
    {
        var expectedOrder = 1;
        foreach (var exercise in exercises.OrderBy(x => x.Order.Value))
        {
            if (exercise.Order.Value != expectedOrder)
            {
                throw new ArgumentException("Session exercise order must be sequential, starting at 1.", nameof(exercises));
            }

            expectedOrder++;
        }
    }

    private static void EnsureUniqueExerciseOrder(IReadOnlyCollection<SessionExercise> exercises)
    {
        if (exercises.Count != exercises.Select(x => x.Order.Value).Distinct().Count())
        {
            throw new ArgumentException("Session exercise order values must be unique.", nameof(exercises));
        }
    }
}
