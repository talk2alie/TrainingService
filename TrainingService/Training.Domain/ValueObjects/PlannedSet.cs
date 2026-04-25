using Training.Domain.Common;

namespace Training.Domain.ValueObjects;

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
        Weight = weight;
        Duration = duration;
        Distance = distance;
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

        if (repetitions is null && duration is null && distance is null)
        {
            throw new ArgumentException("At least one performance target (repetitions, duration, or distance) must be provided.");
        }

        return new PlannedSet(order, repetitions, weight, duration, distance, targetRpe);
    }
}
