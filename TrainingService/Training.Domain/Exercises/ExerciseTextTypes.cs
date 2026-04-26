using Training.Domain.Common;

namespace Training.Domain.Exercises;

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
        const int maxLength = 120;
        Guard.AgainstTooLong(normalized, maxLength, nameof(value));
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
        const int maxLength = 120;
        Guard.AgainstTooLong(normalized, maxLength, nameof(value));
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
        const int maxLength = 2000;
        Guard.AgainstTooLong(normalized, maxLength, nameof(value));
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
        const int maxLength = 2000;
        Guard.AgainstTooLong(normalized, maxLength, nameof(value));
        return new MuscleGroupDescription(normalized);
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}
