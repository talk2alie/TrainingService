namespace Training.Domain.Exercises;

/// <summary>
/// Specifies the binary format for exercise and muscle group images.
/// These formats represent the actual encoded bytes stored in <see cref="ImageData"/>.
/// </summary>
public enum ImageFormat
{
    /// <summary>
    /// PNG (Portable Network Graphics).
    /// Lossless compression; ideal for icons, diagrams, and images requiring transparency.
    /// Larger file sizes but high fidelity.
    /// </summary>
    Png = 1,

    /// <summary>
    /// JPEG (Joint Photographic Experts Group).
    /// Lossy compression; ideal for photos and thumbnails where small size matters.
    /// No transparency support.
    /// </summary>
    Jpeg = 2,

    /// <summary>
    /// WebP (Google Web Picture format).
    /// Modern, efficient format supporting both lossy and lossless compression.
    /// Smaller file sizes with high quality; recommended for most UI images.
    /// </summary>
    Webp = 3,

    /// <summary>
    /// GIF (Graphics Interchange Format).
    /// Supports simple animations and limited color palettes.
    /// Rarely used for exercise thumbnails but included for completeness.
    /// </summary>
    Gif = 4
}
