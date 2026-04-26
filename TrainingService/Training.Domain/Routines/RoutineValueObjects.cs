using Training.Domain.Common;
using Training.Domain.Sessions;

namespace Training.Domain.Routines;

/// <summary>
/// Represents a routine name.
/// </summary>
public sealed record RoutineName
{
    /// <summary>
    /// Gets the normalized routine name.
    /// </summary>
    public string Value { get; }

    private RoutineName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="RoutineName"/>.
    /// </summary>
    public static RoutineName Create(string value)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        const int maxLength = 120;
        Guard.AgainstTooLong(normalized, maxLength, nameof(value));
        return new RoutineName(normalized);
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}

/// <summary>
/// Represents a routine description.
/// </summary>
public sealed record RoutineDescription
{
    /// <summary>
    /// Gets the description value.
    /// </summary>
    public string Value { get; }

    private RoutineDescription(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="RoutineDescription"/>.
    /// </summary>
    public static RoutineDescription Create(string value)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        const int maxLength = 2000;
        Guard.AgainstTooLong(normalized, maxLength, nameof(value));
        return new RoutineDescription(normalized);
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}

/// <summary>
/// Represents a planned set definition for a routine exercise.
/// </summary>
public sealed record PlannedSet
{
    /// <summary>
    /// Gets the order of the planned set.
    /// </summary>
    public Order Order { get; }

    /// <summary>
    /// Gets the planned repetitions.
    /// </summary>
    public int? Repetitions { get; }

    /// <summary>
    /// Gets the planned weight.
    /// </summary>
    public Weight? Weight { get; }

    /// <summary>
    /// Gets the planned duration.
    /// </summary>
    public Duration? Duration { get; }

    /// <summary>
    /// Gets the planned distance.
    /// </summary>
    public Distance? Distance { get; }

    /// <summary>
    /// Gets the planned RPE target.
    /// </summary>
    public RpeScale? TargetRpe { get; }

    private PlannedSet(Order order, int? repetitions, Weight? weight, Duration? duration, Distance? distance, RpeScale? targetRpe)
    {
        Order = order;
        Repetitions = repetitions;

        if (weight is not null && repetitions is null && duration is null && distance is null)
        {
            throw new ArgumentException("Weight cannot be provided without a performance target (repetitions, duration, or distance).", nameof(weight));
        }

        if (weight is { Kilograms: <= 0 })
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be greater than zero.");
        }

        Weight = weight;

        if (duration is { Value: var durationValue } && durationValue <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be greater than zero seconds.");
        }

        Duration = duration;

        if (distance is { Meters: <= 0 })
        {
            throw new ArgumentOutOfRangeException(nameof(distance), "Distance must be greater than zero meters.");
        }

        Distance = distance;

        if (targetRpe.HasValue && !Enum.IsDefined(targetRpe.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(targetRpe), "Target RPE must be a valid predefined value.");
        }

        TargetRpe = targetRpe;
    }

    /// <summary>
    /// Creates a new <see cref="PlannedSet"/>.
    /// </summary>
    public static PlannedSet Create(Order order, int? repetitions = null, Weight? weight = null, Duration? duration = null, Distance? distance = null, RpeScale? targetRpe = null)
    {
        Guard.AgainstNull(order, nameof(order));

        if (repetitions is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(repetitions), "Repetitions must be greater than zero when provided.");
        }

        if (weight is { Kilograms: <= 0 })
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Weight must be greater than zero.");
        }

        if (duration is { Value: var durationValue } && durationValue <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be greater than zero seconds.");
        }

        if (distance is { Meters: <= 0 })
        {
            throw new ArgumentOutOfRangeException(nameof(distance), "Distance must be greater than zero meters.");
        }

        if (targetRpe.HasValue && !Enum.IsDefined(targetRpe.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(targetRpe), "Target RPE must be a valid predefined value.");
        }

        if (weight is not null && repetitions is null && duration is null && distance is null)
        {
            throw new ArgumentException("Weight cannot be provided without a performance target (repetitions, duration, or distance).", nameof(weight));
        }

        if (repetitions is null && duration is null && distance is null)
        {
            throw new ArgumentException("At least one performance target (repetitions, duration, or distance) must be provided.");
        }

        return new PlannedSet(order, repetitions, weight, duration, distance, targetRpe);
    }

    /// <summary>
    /// Returns a copy of this planned set with a different order value.
    /// </summary>
    public PlannedSet WithOrder(Order newOrder)
    {
        Guard.AgainstNull(newOrder, nameof(newOrder));
        return new PlannedSet(newOrder, Repetitions, Weight, Duration, Distance, TargetRpe);
    }
}
