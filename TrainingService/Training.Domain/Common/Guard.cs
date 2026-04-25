namespace Training.Domain.Common;

/// <summary>
/// Provides guard clause helpers for enforcing domain invariants.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Ensures the provided value is not null.
    /// </summary>
    public static T AgainstNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }

        return value;
    }

    /// <summary>
    /// Ensures the provided string is not null, empty, or whitespace.
    /// </summary>
    public static string AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
        }

        return value.Trim();
    }

    /// <summary>
    /// Ensures the provided value is strictly greater than zero.
    /// </summary>
    public static int AgainstNonPositive(int value, string paramName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Value must be greater than zero.");
        }

        return value;
    }

    /// <summary>
    /// Ensures the provided value is strictly greater than zero.
    /// </summary>
    public static decimal AgainstNonPositive(decimal value, string paramName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Value must be greater than zero.");
        }

        return value;
    }

    /// <summary>
    /// Ensures the provided value is zero or positive.
    /// </summary>
    public static decimal AgainstNegative(decimal value, string paramName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Value cannot be negative.");
        }

        return value;
    }

    /// <summary>
    /// Ensures the provided value is within the inclusive range.
    /// </summary>
    public static int AgainstOutOfRange(int value, int minInclusive, int maxInclusive, string paramName)
    {
        if (value < minInclusive || value > maxInclusive)
        {
            throw new ArgumentOutOfRangeException(paramName, $"Value must be between {minInclusive} and {maxInclusive}.");
        }

        return value;
    }

    /// <summary>
    /// Ensures the provided string does not exceed the maximum length.
    /// </summary>
    public static string AgainstTooLong(string value, int maxLength, string paramName)
    {
        if (value.Length > maxLength)
        {
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", paramName);
        }

        return value;
    }

    /// <summary>
    /// Ensures the provided identifier is not empty.
    /// </summary>
    public static Guid AgainstEmptyGuid(Guid value, string paramName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Identifier cannot be empty.", paramName);
        }

        return value;
    }
}
