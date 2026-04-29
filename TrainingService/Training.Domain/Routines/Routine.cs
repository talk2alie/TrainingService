using Training.Domain.Common;
using Training.Domain.Exercises;

namespace Training.Domain.Routines;

/// <summary>
/// Represents a reusable training routine aggregate root.
/// </summary>
public sealed class Routine
{
    private readonly List<RoutineExercise> _exercises = null!;
    private readonly List<IDomainEvent> _domainEvents = [];
    /// <summary>
    /// Ensures all aggregate-level invariants are satisfied.
    /// </summary>
    private void Revalidate()
    {
        // No two RoutineExercise items may have the same RoutineExercise.Id
        var routineExerciseIds = new HashSet<Guid>();
        foreach (var e in _exercises)
        {
            if (!routineExerciseIds.Add(e.Id))
            {
                throw new RoutineInvariantViolationException("Duplicate RoutineExercise.Id detected.");
            }
        }

        // No two RoutineExercise items may have the same ExerciseId
        var exerciseIds = new HashSet<Guid>();
        foreach (var e in _exercises)
        {
            if (!exerciseIds.Add(e.ExerciseId))
            {
                throw new RoutineInvariantViolationException("Duplicate ExerciseId detected.");
            }
        }

        // No two RoutineExercise items may have the same ExerciseName
        var exerciseNames = new HashSet<string>();
        foreach (var e in _exercises)
        {
            if (!exerciseNames.Add(e.ExerciseName.ToString()))
            {
                throw new RoutineInvariantViolationException("Duplicate ExerciseName detected.");
            }
        }

        // No two RoutineExercise items may have the same Order
        var orders = new HashSet<int>();
        foreach (var e in _exercises)
        {
            if (!orders.Add(e.Order.Value))
            {
                throw new RoutineInvariantViolationException("Duplicate exercise order detected.");
            }
        }

        // Orders must always be sequential starting at 1
        var orderValues = _exercises.Select(e => e.Order.Value).OrderBy(x => x).ToArray();
        for (int i = 0; i < orderValues.Length; i++)
        {
            if (orderValues[i] != i + 1)
            {
                throw new RoutineInvariantViolationException("Exercise orders must be sequential starting from 1.");
            }
        }
    }

    /// <summary>
    /// Normalizes the order values of all RoutineExercise items to be sequential starting at 1.
    /// </summary>
    private void NormalizeOrders()
    {
        for (int i = 0; i < _exercises.Count; i++)
        {
            _exercises[i].SetOrder(Order.Create(i + 1));
        }
    }

    private Routine(
        Guid id,
        Guid ownerUserId,
        DateTimeOffset createdAtUtc,
        RoutineName name,
        RoutineDescription description,
        DifficultyLevel difficultyLevel,
        IEnumerable<RoutineExercise> exercises)
    {
        Id = Guard.AgainstEmptyGuid(id, nameof(id));
        OwnerUserId = Guard.AgainstEmptyGuid(ownerUserId, nameof(ownerUserId));

        if (createdAtUtc.Offset != TimeSpan.Zero)
        {
            throw new RoutineInvariantViolationException("Creation time must be in UTC.", nameof(createdAtUtc));
        }

        CreatedAtUtc = createdAtUtc;
        Name = Guard.AgainstNull(name, nameof(name));
        Description = Guard.AgainstNull(description, nameof(description));

        if (!Enum.IsDefined(difficultyLevel))
        {
            throw new RoutineInvariantViolationException("Difficulty level is not defined.", nameof(difficultyLevel));
        }

        DifficultyLevel = difficultyLevel;

        ArgumentNullException.ThrowIfNull(exercises);
        _exercises = [.. exercises];

        NormalizeOrders();
        Revalidate();
    }

    private Routine() { }

    /// <summary>
    /// Gets the routine identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the routine owner identifier.
    /// </summary>
    public Guid OwnerUserId { get; }

    /// <summary>
    /// Gets the UTC timestamp when the routine was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; }

    /// <summary>
    /// Gets the routine name.
    /// </summary>
    public RoutineName Name { get; private set; } = null!;

    /// <summary>
    /// Gets the routine description.
    /// </summary>
    public RoutineDescription Description { get; private set; } = null!;

    /// <summary>
    /// Gets the routine planned difficulty level.
    /// </summary>
    public DifficultyLevel DifficultyLevel { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the routine was archived, if archived.
    /// </summary>
    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the routine is archived.
    /// </summary>
    public bool IsArchived => ArchivedAtUtc.HasValue;

    /// <summary>
    /// Gets the routine exercises.
    /// </summary>
    public IReadOnlyList<RoutineExercise> Exercises => _exercises.AsReadOnly();

    /// <summary>
    /// Gets the domain events raised by this routine.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Creates a new <see cref="Routine"/>.
    /// </summary>
    public static Routine Create(
        Guid ownerUserId,
        RoutineName name,
        RoutineDescription description,
        DifficultyLevel difficultyLevel,
        IEnumerable<RoutineExercise> exercises)
    {
        var routine = new Routine(
            Guid.NewGuid(),
            ownerUserId,
            DateTimeOffset.UtcNow,
            name,
            description,
            difficultyLevel,
            exercises);
        routine.Raise(new RoutineCreated(
            routine.Id,
            routine.Name,
            routine.OwnerUserId,
            DateTimeOffset.UtcNow));
        return routine;
    }

    /// <summary>
    /// Renames the routine.
    /// </summary>
    public void Rename(RoutineName name)
    {
        EnsureNotArchived();
        Name = Guard.AgainstNull(name, nameof(name));
        Revalidate();
    }

    /// <summary>
    /// Updates the routine description.
    /// </summary>
    public void UpdateDescription(RoutineDescription description)
    {
        EnsureNotArchived();
        Description = Guard.AgainstNull(description, nameof(description));
        Revalidate();
    }

    /// <summary>
    /// Updates the routine planned difficulty level.
    /// </summary>
    public void UpdateDifficultyLevel(DifficultyLevel difficultyLevel)
    {
        EnsureNotArchived();
        DifficultyLevel = difficultyLevel;
        Revalidate();
    }

    /// <summary>
    /// Archives the routine.
    /// </summary>
    public void Archive()
    {
        EnsureNotArchived();
        if (_exercises.Count == 0)
        {
            throw new InvalidRoutineOperationException("Cannot archive a routine with no exercises.");
        }

        var nowUtc = DateTimeOffset.UtcNow;
        if (nowUtc < CreatedAtUtc)
        {
            throw new RoutineInvariantViolationException("Archive time cannot be earlier than creation time.");
        }

        ArchivedAtUtc = nowUtc;
        Raise(new RoutineArchived(Id, nowUtc));
        Revalidate();
    }

    /// <summary>
    /// Adds a new exercise to the routine.
    /// </summary>
    public Guid AddExercise(Guid exerciseId, ExerciseName exerciseName, IEnumerable<PlannedSet> plannedSets)
    {
        EnsureNotArchived();

        var nextOrder = _exercises.Count == 0 ? 1 : _exercises.Max(e => e.Order.Value) + 1;
        var routineExercise = RoutineExercise.Create(exerciseId, exerciseName, Order.Create(nextOrder), plannedSets);
        _exercises.Add(routineExercise);
        NormalizeOrders();
        Revalidate();
        return routineExercise.Id;
    }

    /// <summary>
    /// Removes an exercise from the routine.
    /// </summary>
    public void RemoveExercise(Guid routineExerciseId)
    {
        EnsureNotArchived();

        if (_exercises.Count == 1)
        {
            throw new InvalidRoutineOperationException("A routine must contain at least one exercise.");
        }

        var exercise = _exercises.FirstOrDefault(e => e.Id == routineExerciseId);
        if (exercise is null)
        {
            throw new InvalidRoutineOperationException("Routine exercise not found.");
        }

        _exercises.Remove(exercise);
        NormalizeOrders();
        Revalidate();
    }

    /// <summary>
    /// Reorders a routine exercise.
    /// </summary>
    public void MoveExercise(Guid routineExerciseId, Order newOrder)
    {
        EnsureNotArchived();

        var exercise = _exercises.FirstOrDefault(e => e.Id == routineExerciseId);
        if (exercise is null)
        {
            throw new InvalidRoutineOperationException("Routine exercise not found.");
        }

        if (newOrder.Value < 1 || newOrder.Value > _exercises.Count)
        {
            throw new InvalidRoutineOperationException("Invalid order value.");
        }

        _exercises.Remove(exercise);
        _exercises.Insert(newOrder.Value - 1, exercise);
        NormalizeOrders();
        Revalidate();
    }

    /// <summary>
    /// Renames an exercise snapshot in the routine.
    /// </summary>
    public void RenameExercise(Guid routineExerciseId, ExerciseName exerciseName)
    {
        EnsureNotArchived();
        Guard.AgainstEmptyGuid(routineExerciseId, nameof(routineExerciseId));
        Guard.AgainstNull(exerciseName, nameof(exerciseName));

        var exercise = _exercises.FirstOrDefault(x => x.Id == routineExerciseId);
        if (exercise is null)
        {
            throw new InvalidRoutineOperationException("Routine exercise not found.");
        }

        if (exercise.ExerciseName.Equals(exerciseName))
        {
            throw new InvalidRoutineOperationException("Routine exercise name is already set to the requested value.");
        }

        exercise.RenameExercise(exerciseName);
    }

    /// <summary>
    /// Adds a planned set to a routine exercise.
    /// </summary>
    public void AddPlannedSet(Guid routineExerciseId, PlannedSet plannedSet)
    {
        EnsureNotArchived();
        Guard.AgainstEmptyGuid(routineExerciseId, nameof(routineExerciseId));
        Guard.AgainstNull(plannedSet, nameof(plannedSet));

        var exercise = _exercises.FirstOrDefault(x => x.Id == routineExerciseId);
        if (exercise is null)
        {
            throw new InvalidRoutineOperationException("Routine exercise not found.");
        }

        if (exercise.PlannedSets.Any(x => x.Order.Value == plannedSet.Order.Value))
        {
            throw new InvalidRoutineOperationException($"Routine exercise already contains a planned set with order {plannedSet.Order.Value}.");
        }

        exercise.AddPlannedSet(plannedSet);
    }

    /// <summary>
    /// Removes a planned set from a routine exercise by set order.
    /// </summary>
    public void RemovePlannedSet(Guid routineExerciseId, Order setOrder)
    {
        EnsureNotArchived();
        Guard.AgainstEmptyGuid(routineExerciseId, nameof(routineExerciseId));
        Guard.AgainstNull(setOrder, nameof(setOrder));

        var exercise = _exercises.FirstOrDefault(x => x.Id == routineExerciseId);
        if (exercise is null)
        {
            throw new InvalidRoutineOperationException("Routine exercise not found.");
        }

        if (exercise.PlannedSets.All(x => x.Order != setOrder))
        {
            throw new InvalidRoutineOperationException("The planned set was not found for the routine exercise.");
        }

        exercise.RemovePlannedSet(setOrder);
    }

    private void EnsureNotArchived()
    {
        if (IsArchived)
        {
            throw new InvalidRoutineOperationException("Cannot modify an archived routine.");
        }
    }


    private void Raise(IDomainEvent @event)
    {
        _domainEvents.Add(@event);
    }
}
