namespace Training.Domain.Common;

/// <summary>
/// Represents a weight value in kilograms.
/// </summary>
public sealed record Weight
{
    /// <summary>
    /// Gets the value in kilograms.
    /// </summary>
    public decimal Kilograms { get; }

    private Weight(decimal kilograms)
    {
        Kilograms = kilograms;
    }

    /// <summary>
    /// Creates a new <see cref="Weight"/>.
    /// </summary>
    public static Weight Create(decimal kilograms)
    {
        Guard.AgainstNegative(kilograms, nameof(kilograms));
        return new Weight(decimal.Round(kilograms, 2, MidpointRounding.AwayFromZero));
    }
}

/// <summary>
/// Represents a duration value.
/// </summary>
public sealed record Duration
{
    /// <summary>
    /// Gets the duration.
    /// </summary>
    public TimeSpan Value { get; }

    private Duration(TimeSpan value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/>.
    /// </summary>
    public static Duration Create(TimeSpan value)
    {
        if (value <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Duration must be greater than zero.");
        }

        return new Duration(value);
    }
}

/// <summary>
/// Represents a distance value in meters.
/// </summary>
public sealed record Distance
{
    /// <summary>
    /// Gets the value in meters.
    /// </summary>
    public decimal Meters { get; }

    private Distance(decimal meters)
    {
        Meters = meters;
    }

    /// <summary>
    /// Creates a new <see cref="Distance"/>.
    /// </summary>
    public static Distance Create(decimal meters)
    {
        Guard.AgainstNegative(meters, nameof(meters));
        return new Distance(decimal.Round(meters, 2, MidpointRounding.AwayFromZero));
    }
}

/// <summary>
/// Represents a rate of perceived exertion value.
/// </summary>
public sealed record RpeScale
{
    /// <summary>
    /// Gets the scale value from 1 to 10.
    /// </summary>
    public int Value { get; }

    private RpeScale(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="RpeScale"/>.
    /// </summary>
    public static RpeScale Create(int value)
    {
        Guard.AgainstOutOfRange(value, 1, 10, nameof(value));
        return new RpeScale(value);
    }
}

/// <summary>
/// Represents heart rate in beats per minute.
/// </summary>
public sealed record HeartRate
{
    /// <summary>
    /// Gets the beats per minute.
    /// </summary>
    public int BeatsPerMinute { get; }

    private HeartRate(int beatsPerMinute)
    {
        BeatsPerMinute = beatsPerMinute;
    }

    /// <summary>
    /// Creates a new <see cref="HeartRate"/>.
    /// </summary>
    public static HeartRate Create(int beatsPerMinute)
    {
        Guard.AgainstOutOfRange(beatsPerMinute, 20, 260, nameof(beatsPerMinute));
        return new HeartRate(beatsPerMinute);
    }
}

/// <summary>
/// Represents a one-based order value.
/// </summary>
public sealed record Order
{
    /// <summary>
    /// Gets the one-based sequence number.
    /// </summary>
    public int Value { get; }

    private Order(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="Order"/>.
    /// </summary>
    public static Order Create(int value)
    {
        Guard.AgainstNonPositive(value, nameof(value));
        return new Order(value);
    }
}
