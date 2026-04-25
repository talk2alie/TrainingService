using Training.Domain.Common;

namespace Training.Domain.ValueObjects;

/// <summary>
/// Represents an exercise name.
/// </summary>
public sealed record ExerciseName
{
    /// <summary>
    /// Gets the normalized exercise name.
    /// </summary>
    public string Value { get; }

    private ExerciseName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="ExerciseName"/>.
    /// </summary>
    public static ExerciseName Create(string value)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        Guard.AgainstTooLong(normalized, 120, nameof(value));
        return new ExerciseName(normalized);
    }

    /// <inheritdoc />
    public bool Equals(ExerciseName? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null)
        {
            return false;
        }

        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    /// <inheritdoc />
    public override string ToString() => Value;
}

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
        Guard.AgainstTooLong(normalized, 120, nameof(value));
        return new RoutineName(normalized);
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}

/// <summary>
/// Represents a muscle group name.
/// </summary>
public sealed record MuscleGroupName
{
    /// <summary>
    /// Gets the normalized muscle group name.
    /// </summary>
    public string Value { get; }

    private MuscleGroupName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="MuscleGroupName"/>.
    /// </summary>
    public static MuscleGroupName Create(string value)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        Guard.AgainstTooLong(normalized, 120, nameof(value));
        return new MuscleGroupName(normalized);
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}

/// <summary>
/// Represents an exercise description.
/// </summary>
public sealed record ExerciseDescription
{
    /// <summary>
    /// Gets the description value.
    /// </summary>
    public string Value { get; }

    private ExerciseDescription(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="ExerciseDescription"/>.
    /// </summary>
    public static ExerciseDescription Create(string value)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        Guard.AgainstTooLong(normalized, 2000, nameof(value));
        return new ExerciseDescription(normalized);
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}

/// <summary>
/// Represents a muscle group description.
/// </summary>
public sealed record MuscleGroupDescription
{
    /// <summary>
    /// Gets the description value.
    /// </summary>
    public string Value { get; }

    private MuscleGroupDescription(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="MuscleGroupDescription"/>.
    /// </summary>
    public static MuscleGroupDescription Create(string value)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        Guard.AgainstTooLong(normalized, 2000, nameof(value));
        return new MuscleGroupDescription(normalized);
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}

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
