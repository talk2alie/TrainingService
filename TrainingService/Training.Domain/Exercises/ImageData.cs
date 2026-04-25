namespace Training.Domain.Exercises;

/// <summary>
/// Represents immutable image binary data.
/// </summary>
public sealed class ImageData : IEquatable<ImageData>
{
    /// <summary>
    /// Gets the image bytes.
    /// </summary>
    public IReadOnlyList<byte> Bytes { get; }

    /// <summary>
    /// Gets the image format.
    /// </summary>
    public ImageFormat Format { get; }

    private ImageData(byte[] bytes, ImageFormat format)
    {
        Bytes = Array.AsReadOnly(bytes);
        Format = format;
    }

    /// <summary>
    /// Creates a new <see cref="ImageData"/>.
    /// </summary>
    public static ImageData Create(byte[] bytes, ImageFormat format)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        if (bytes.Length == 0)
        {
            throw new ArgumentException("Image bytes cannot be empty.", nameof(bytes));
        }

        return new ImageData([.. bytes], format);
    }

    /// <inheritdoc />
    public bool Equals(ImageData? other)
    {
        if (other is null)
        {
            return false;
        }

        return Format == other.Format && Bytes.SequenceEqual(other.Bytes);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ImageData other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Format);

        foreach (var b in Bytes)
        {
            hash.Add(b);
        }

        return hash.ToHashCode();
    }
}
