using Training.Domain.Common;
using Training.Domain.Exercises;

namespace Training.Domain.Routines;

/// <summary>
/// Represents a reusable training routine aggregate root.
/// </summary>
public sealed class Routine
{
    private readonly List<RoutineExercise> _exercises;

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
        DifficultyLevel = difficultyLevel;

        ArgumentNullException.ThrowIfNull(exercises);
        _exercises = [.. exercises];

        EnsureAtLeastOneExercise(_exercises);
        EnsureRoutineExerciseIdsAreUnique(_exercises);
        EnsureRoutineExerciseInvariants(_exercises);
        EnsureNoDuplicateExercises(_exercises);
        EnsureUniqueExerciseOrder(_exercises);
        EnsureSequentialExerciseOrder(_exercises);
        NormalizeOrders();
    }

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
    public RoutineName Name { get; private set; }

    /// <summary>
    /// Gets the routine description.
    /// </summary>
    public RoutineDescription Description { get; private set; }

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
    /// Creates a new <see cref="Routine"/>.
    /// </summary>
    public static Routine Create(
        Guid ownerUserId,
        RoutineName name,
        RoutineDescription description,
        DifficultyLevel difficultyLevel,
        Guid firstExerciseId,
        ExerciseName firstExerciseName,
        IEnumerable<PlannedSet> firstExercisePlannedSets)
    {
        ArgumentNullException.ThrowIfNull(firstExercisePlannedSets);
        var firstExercisePlannedSetsList = firstExercisePlannedSets.ToList();
        EnsureValidPlannedSets(firstExercisePlannedSetsList, nameof(firstExercisePlannedSets));

        var firstExercise = RoutineExercise.Create(
            firstExerciseId,
            firstExerciseName,
            Order.Create(1),
            firstExercisePlannedSetsList);

        return new Routine(Guid.NewGuid(), ownerUserId, DateTimeOffset.UtcNow, name, description, difficultyLevel, [firstExercise]);
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
    public void Archive(DateTimeOffset archivedAtUtc)
    {
        EnsureNotArchived();

        if (archivedAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Routine archive timestamp must be provided in UTC.", nameof(archivedAtUtc));
        }

        if (archivedAtUtc < CreatedAtUtc)
        {
            throw new InvalidOperationException("Routine archive timestamp cannot be earlier than routine creation time.");
        }

        var nowUtc = DateTimeOffset.UtcNow;
        var minAllowedArchiveTimeUtc = nowUtc.AddMinutes(-1);
        var maxAllowedArchiveTimeUtc = nowUtc.AddMinutes(5);

        if (archivedAtUtc < minAllowedArchiveTimeUtc || archivedAtUtc > maxAllowedArchiveTimeUtc)
        {
            throw new InvalidOperationException("Routine archive timestamp is outside the allowed window (from 1 minute in the past to 5 minutes in the future).");
        }

        ArchivedAtUtc = archivedAtUtc;
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
        return _exercises.SingleOrDefault(x => x.Id == routineExerciseId)
            ?? throw new InvalidOperationException("The routine exercise was not found.");
    }

    private void EnsureNotArchived()
    {
        if (IsArchived)
        {
            throw new InvalidOperationException("Cannot modify an archived routine.");
        }
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

    private static void EnsureAtLeastOneExercise(IReadOnlyCollection<RoutineExercise> exercises)
    {
        if (exercises.Count == 0)
        {
            throw new ArgumentException("At least one routine exercise must be provided.", nameof(exercises));
        }
    }

    private static void EnsureNoDuplicateExercises(IReadOnlyCollection<RoutineExercise> exercises)
    {
        if (exercises.Count != exercises.Select(x => x.ExerciseId).Distinct().Count())
        {
            throw new ArgumentException("Routine cannot contain duplicate exercise references (ExerciseId).", nameof(exercises));
        }
    }

    private static void EnsureSequentialExerciseOrder(IReadOnlyCollection<RoutineExercise> exercises)
    {
        var expectedOrder = 1;
        foreach (var exercise in exercises.OrderBy(x => x.Order.Value))
        {
            if (exercise.Order.Value != expectedOrder)
            {
                throw new ArgumentException("Routine exercise order must be sequential, starting at 1.", nameof(exercises));
            }

            expectedOrder++;
        }
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
        if (exercises.Count != exercises.Select(x => x.Order.Value).Distinct().Count())
        {
            throw new ArgumentException("Routine exercise order values must be unique within the routine.", nameof(exercises));
        }
    }

    private static void EnsureRoutineExerciseIdsAreUnique(IReadOnlyCollection<RoutineExercise> exercises)
    {
        if (exercises.Count != exercises.Select(x => x.Id).Distinct().Count())
        {
            throw new ArgumentException("Routine contains duplicate routine exercise entity identifiers, which indicates invalid aggregate composition.", nameof(exercises));
        }
    }

    private static void EnsureRoutineExerciseInvariants(IReadOnlyCollection<RoutineExercise> exercises)
    {
        foreach (var exercise in exercises)
        {
            if (exercise.Order.Value < 1)
            {
                throw new ArgumentException("Each routine exercise must have an order greater than or equal to 1.", nameof(exercises));
            }

            EnsureValidPlannedSets(exercise.PlannedSets, nameof(exercises));
        }
    }
}
