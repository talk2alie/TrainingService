using System.Collections.Generic;
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
            throw new ArgumentException("Creation time must be in UTC.", nameof(createdAtUtc));
        }

        CreatedAtUtc = createdAtUtc;
        Name = Guard.AgainstNull(name, nameof(name));
        Description = Guard.AgainstNull(description, nameof(description));

        if (!Enum.IsDefined(difficultyLevel))
        {
            throw new ArgumentOutOfRangeException(nameof(difficultyLevel));
        }

        DifficultyLevel = difficultyLevel;

        ArgumentNullException.ThrowIfNull(exercises);
        _exercises = [.. exercises];

        EnsureRoutineExerciseIdsAreUnique(_exercises);
        EnsureRoutineExerciseInvariants(_exercises);
        EnsureNoDuplicateExercises(_exercises);
        EnsureUniqueExerciseOrder(_exercises);
        EnsureSequentialExerciseOrder(_exercises);
        NormalizeOrders();
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
    }

    /// <summary>
    /// Updates the routine description.
    /// </summary>
    public void UpdateDescription(RoutineDescription description)
    {
        EnsureNotArchived();
        Description = Guard.AgainstNull(description, nameof(description));
    }

    /// <summary>
    /// Updates the routine planned difficulty level.
    /// </summary>
    public void UpdateDifficultyLevel(DifficultyLevel difficultyLevel)
    {
        EnsureNotArchived();
        DifficultyLevel = difficultyLevel;
    }

    /// <summary>
    /// Archives the routine.
    /// </summary>
    public void Archive()
    {
        EnsureNotArchived();
        if (_exercises.Count == 0)
            throw new InvalidOperationException("Cannot archive a routine with no exercises.");

        var nowUtc = DateTimeOffset.UtcNow;
        if (nowUtc < CreatedAtUtc)
            throw new InvalidOperationException("Archive time cannot be earlier than creation time.");

        ArchivedAtUtc = nowUtc;
        Raise(new RoutineArchived(Id, nowUtc));
    }

    /// <summary>
    /// Adds a new exercise to the routine.
    /// </summary>
    public Guid AddExercise(Guid exerciseId, ExerciseName exerciseName, IEnumerable<PlannedSet> plannedSets)
    {
        EnsureNotArchived();
        Guard.AgainstEmptyGuid(exerciseId, nameof(exerciseId));
        Guard.AgainstNull(exerciseName, nameof(exerciseName));
        ArgumentNullException.ThrowIfNull(plannedSets);
        var plannedSetsList = plannedSets.ToList();
        EnsureValidPlannedSets(plannedSetsList, nameof(plannedSets));

        if (_exercises.Any(x => x.ExerciseId == exerciseId))
        {
            throw new InvalidOperationException("The routine already contains this exercise.");
        }

        var nextOrder = Order.Create(_exercises.Count + 1);
        var routineExercise = RoutineExercise.Create(exerciseId, exerciseName, nextOrder, plannedSetsList);
        _exercises.Add(routineExercise);
        return routineExercise.Id;
    }

    /// <summary>
    /// Removes an exercise from the routine.
    /// </summary>
    public void RemoveExercise(Guid routineExerciseId)
    {
        EnsureNotArchived();
        Guard.AgainstEmptyGuid(routineExerciseId, nameof(routineExerciseId));
        if (_exercises.Count == 1)
        {
            throw new InvalidOperationException("A routine must contain at least one exercise.");
        }
        var exercise = GetExerciseById(routineExerciseId);
        _exercises.Remove(exercise);
        NormalizeOrders();
    }

    /// <summary>
    /// Reorders a routine exercise.
    /// </summary>
    public void MoveExercise(Guid routineExerciseId, Order targetOrder)
    {
        EnsureNotArchived();
        Guard.AgainstEmptyGuid(routineExerciseId, nameof(routineExerciseId));
        Guard.AgainstNull(targetOrder, nameof(targetOrder));
        if (targetOrder.Value > _exercises.Count)
        {
            throw new InvalidOperationException("Target order exceeds exercise count.");
        }
        var exercise = GetExerciseById(routineExerciseId);
        if (exercise.Order.Value == targetOrder.Value)
        {
            throw new InvalidOperationException("Routine exercise is already at the requested target order.");
        }
        var sorted = _exercises.OrderBy(x => x.Order.Value).ToList();
        sorted.Remove(exercise);
        sorted.Insert(targetOrder.Value - 1, exercise);
        for (int i = 0; i < sorted.Count; i++)
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
        EnsureNotArchived();
        Guard.AgainstEmptyGuid(routineExerciseId, nameof(routineExerciseId));
        Guard.AgainstNull(exerciseName, nameof(exerciseName));

        var exercise = GetExerciseById(routineExerciseId);

        if (exercise.ExerciseName.Equals(exerciseName))
        {
            throw new InvalidOperationException("Routine exercise name is already set to the requested value.");
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

        var exercise = GetExerciseById(routineExerciseId);

        if (exercise.PlannedSets.Any(x => x.Order.Value == plannedSet.Order.Value))
        {
            throw new InvalidOperationException($"Routine exercise already contains a planned set with order {plannedSet.Order.Value}.");
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

        var exercise = GetExerciseById(routineExerciseId);

        if (exercise.PlannedSets.All(x => x.Order != setOrder))
        {
            throw new InvalidOperationException("The planned set was not found for the routine exercise.");
        }

        exercise.RemovePlannedSet(setOrder);
    }

    private RoutineExercise GetExerciseById(Guid routineExerciseId)
    {
        var exercise = _exercises.SingleOrDefault(x => x.Id == routineExerciseId);
        if (exercise is null)
            throw new InvalidOperationException("Routine exercise not found.");
        return exercise;
    }

    private void EnsureNotArchived()
    {
        if (IsArchived)
            throw new InvalidOperationException("Cannot modify an archived routine.");
    }

    private static void EnsureValidPlannedSets(IReadOnlyCollection<PlannedSet> plannedSets, string paramName)
    {
        if (plannedSets.Count == 0)
        {
            throw new ArgumentException("At least one planned set must be provided.", paramName);
        }

        if (plannedSets.Count != plannedSets.Select(x => x.Order).Distinct().Count())
        {
            throw new ArgumentException("Planned set order values must be unique.", paramName);
        }

        var expectedOrder = 1;
        foreach (var plannedSet in plannedSets.OrderBy(x => x.Order.Value))
        {
            if (plannedSet.Order.Value != expectedOrder)
            {
                throw new ArgumentException("Planned set order must be sequential, starting at 1.", paramName);
            }

            expectedOrder++;
        }
    }

    // Invariant and normalization helpers (must be present and private)
    private void EnsureRoutineExerciseIdsAreUnique(List<RoutineExercise> exercises)
    {
        if (exercises.Count == 0) return;
        var ids = new HashSet<Guid>();
        foreach (var ex in exercises)
        {
            if (!ids.Add(ex.ExerciseId))
                throw new ArgumentException("Duplicate exercise id detected.");
        }
    }

    private void EnsureRoutineExerciseInvariants(List<RoutineExercise> exercises)
    {
        if (exercises.Count == 0) return;
        // Add any additional invariants here as needed
    }

    private void EnsureNoDuplicateExercises(List<RoutineExercise> exercises)
    {
        if (exercises.Count == 0) return;
        var names = new HashSet<string>();
        foreach (var ex in exercises)
        {
            if (!names.Add(ex.ExerciseName.ToString()))
                throw new ArgumentException("Duplicate exercise name detected.");
        }
    }

    private void EnsureUniqueExerciseOrder(List<RoutineExercise> exercises)
    {
        if (exercises.Count == 0) return;
        var orders = new HashSet<int>();
        foreach (var ex in exercises)
        {
            if (!orders.Add(ex.Order.Value))
                throw new ArgumentException("Duplicate exercise order detected.");
        }
    }

    private void EnsureSequentialExerciseOrder(List<RoutineExercise> exercises)
    {
        if (exercises.Count == 0) return;
        var sorted = exercises.OrderBy(x => x.Order.Value).ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            if (sorted[i].Order.Value != i + 1)
                throw new ArgumentException("Exercise orders must be sequential starting from 1.");
        }
    }

    private void NormalizeOrders()
    {
        if (_exercises.Count == 0) return;
        var sorted = _exercises.OrderBy(x => x.Order.Value).ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].UpdateOrder(Order.Create(i + 1));
        }
        _exercises.Clear();
        _exercises.AddRange(sorted);
    }

    private void Raise(IDomainEvent @event) => _domainEvents.Add(@event);
}
