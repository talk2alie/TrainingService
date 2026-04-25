using Training.Domain.Common;

namespace Training.Domain.Sessions;

/// <summary>
/// Represents a note attached to a session.
/// </summary>
public sealed record SessionNote
{
    /// <summary>
    /// Gets the note value.
    /// </summary>
    public string Value { get; }

    private SessionNote(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="SessionNote"/>.
    /// </summary>
    public static SessionNote Create(string value)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        Guard.AgainstTooLong(normalized, 2000, nameof(value));
        return new SessionNote(normalized);
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}

/// <summary>
/// Represents a note for a logged set.
/// </summary>
public sealed record LogNote
{
    /// <summary>
    /// Gets the note value.
    /// </summary>
    public string Value { get; }

    private LogNote(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="LogNote"/>.
    /// </summary>
    public static LogNote Create(string value)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        Guard.AgainstTooLong(normalized, 1000, nameof(value));
        return new LogNote(normalized);
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}

/// <summary>
/// Represents an actual logged set captured during a session.
/// </summary>
public sealed record LoggedSet
{
    /// <summary>
    /// Gets the order of the logged set.
    /// </summary>
    public Order Order { get; }

    /// <summary>
    /// Gets the logged repetitions.
    /// </summary>
    public int? Repetitions { get; }

    /// <summary>
    /// Gets the logged weight.
    /// </summary>
    public Weight? Weight { get; }

    /// <summary>
    /// Gets the logged duration.
    /// </summary>
    public Duration? Duration { get; }

    /// <summary>
    /// Gets the logged distance.
    /// </summary>
    public Distance? Distance { get; }

    /// <summary>
    /// Gets the logged RPE value.
    /// </summary>
    public RpeScale Rpe { get; }

    /// <summary>
    /// Gets the logged heart rate.
    /// </summary>
    public HeartRate? HeartRate { get; }

    /// <summary>
    /// Gets an optional log note.
    /// </summary>
    public LogNote? Note { get; }

    private LoggedSet(Order order, int? repetitions, Weight? weight, Duration? duration, Distance? distance, RpeScale rpe, HeartRate? heartRate, LogNote? note)
    {
        Order = order;
        Repetitions = repetitions;

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

        if (!Enum.IsDefined(rpe))
        {
            throw new ArgumentOutOfRangeException(nameof(rpe), "RPE must be a valid predefined value.");
        }

        Rpe = rpe;

        if (heartRate is { BeatsPerMinute: <= 0 })
        {
            throw new ArgumentOutOfRangeException(nameof(heartRate), "Heart rate must be greater than zero bpm.");
        }

        HeartRate = heartRate;
        Note = note;
    }

    /// <summary>
    /// Creates a new <see cref="LoggedSet"/>.
    /// </summary>
    public static LoggedSet Create(Order order, RpeScale rpe, int? repetitions = null, Weight? weight = null, Duration? duration = null, Distance? distance = null, HeartRate? heartRate = null, LogNote? note = null)
    {
        Guard.AgainstNull(order, nameof(order));

        if (!Enum.IsDefined(rpe))
        {
            throw new ArgumentOutOfRangeException(nameof(rpe), "RPE must be a valid predefined value.");
        }

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

        if (heartRate is { BeatsPerMinute: <= 0 })
        {
            throw new ArgumentOutOfRangeException(nameof(heartRate), "Heart rate must be greater than zero bpm.");
        }

        if (repetitions is null && weight is null && duration is null && distance is null && heartRate is null)
        {
            throw new ArgumentException("A logged set must contain at least one metric (repetitions, weight, duration, distance, or heart rate).");
        }

        return new LoggedSet(order, repetitions, weight, duration, distance, rpe, heartRate, note);
    }

    /// <summary>
    /// Returns a copy of this logged set with a different order value.
    /// </summary>
    public LoggedSet WithOrder(Order newOrder)
    {
        Guard.AgainstNull(newOrder, nameof(newOrder));
        return new LoggedSet(newOrder, Repetitions, Weight, Duration, Distance, Rpe, HeartRate, Note);
    }

    /// <summary>
    /// Resolves the physiological heart rate zone for this logged set when heart rate is available.
    /// </summary>
    public HeartRateZone? GetZone(HeartRateZoneService service, int age, int? restingHr = null, int? maxHr = null)
    {
        ArgumentNullException.ThrowIfNull(service);

        if (HeartRate is null)
        {
            return null;
        }

        return service.Calculate(HeartRate, age, restingHr, maxHr);
    }
}
