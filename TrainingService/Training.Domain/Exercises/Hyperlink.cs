using Training.Domain.Common;

namespace Training.Domain.Exercises;

/// <summary>
/// Represents an absolute HTTP/HTTPS hyperlink.
/// </summary>
public sealed record Hyperlink
{
    /// <summary>
    /// Gets the absolute URI.
    /// </summary>
    public Uri Value { get; }

    private Hyperlink(Uri value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="Hyperlink"/>.
    /// </summary>
    public static Hyperlink Create(string value)
    {
        var normalized = Guard.AgainstNullOrWhiteSpace(value, nameof(value));

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("A valid absolute URI is required.", nameof(value));
        }

        if (uri.Scheme is not ("http" or "https"))
        {
            throw new ArgumentException("Only HTTP or HTTPS links are supported.", nameof(value));
        }

        return new Hyperlink(uri);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString();
}
