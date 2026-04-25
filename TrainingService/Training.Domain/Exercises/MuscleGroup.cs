using Training.Domain.Common;
using Training.Domain.ValueObjects;

namespace Training.Domain.Exercises;

/// <summary>
/// Represents immutable muscle group reference data as predefined value-object instances.
/// </summary>
public sealed class MuscleGroup : IEquatable<MuscleGroup>
{
    /// <summary>
    /// Chest muscle group.
    /// </summary>
    public static readonly MuscleGroup Chest = new(
        MuscleGroupName.Create("Chest"),
        MuscleGroupDescription.Create("Pectoral muscles responsible for horizontal pressing and adduction movements."));

    /// <summary>
    /// Back muscle group.
    /// </summary>
    public static readonly MuscleGroup Back = new(
        MuscleGroupName.Create("Back"),
        MuscleGroupDescription.Create("Latissimus dorsi and upper back musculature responsible for pulling movements."));

    /// <summary>
    /// Shoulder muscle group.
    /// </summary>
    public static readonly MuscleGroup Shoulders = new(
        MuscleGroupName.Create("Shoulders"),
        MuscleGroupDescription.Create("Deltoid musculature responsible for overhead and lateral arm movement."));

    /// <summary>
    /// Biceps muscle group.
    /// </summary>
    public static readonly MuscleGroup Biceps = new(
        MuscleGroupName.Create("Biceps"),
        MuscleGroupDescription.Create("Elbow flexor muscle group on the anterior upper arm."));

    /// <summary>
    /// Triceps muscle group.
    /// </summary>
    public static readonly MuscleGroup Triceps = new(
        MuscleGroupName.Create("Triceps"),
        MuscleGroupDescription.Create("Elbow extensor muscle group on the posterior upper arm."));

    /// <summary>
    /// Forearm muscle group.
    /// </summary>
    public static readonly MuscleGroup Forearms = new(
        MuscleGroupName.Create("Forearms"),
        MuscleGroupDescription.Create("Forearm flexor and extensor musculature supporting grip and wrist control."));

    /// <summary>
    /// Quadriceps muscle group.
    /// </summary>
    public static readonly MuscleGroup Quadriceps = new(
        MuscleGroupName.Create("Quadriceps"),
        MuscleGroupDescription.Create("Anterior thigh muscle group primarily responsible for knee extension."));

    /// <summary>
    /// Hamstrings muscle group.
    /// </summary>
    public static readonly MuscleGroup Hamstrings = new(
        MuscleGroupName.Create("Hamstrings"),
        MuscleGroupDescription.Create("Posterior thigh muscle group responsible for knee flexion and hip extension."));

    /// <summary>
    /// Glute muscle group.
    /// </summary>
    public static readonly MuscleGroup Glutes = new(
        MuscleGroupName.Create("Glutes"),
        MuscleGroupDescription.Create("Hip extensor and stabilizer muscle group including gluteus maximus, medius, and minimus."));

    /// <summary>
    /// Calf muscle group.
    /// </summary>
    public static readonly MuscleGroup Calves = new(
        MuscleGroupName.Create("Calves"),
        MuscleGroupDescription.Create("Lower leg muscle group responsible for plantar flexion."));

    /// <summary>
    /// Core muscle group.
    /// </summary>
    public static readonly MuscleGroup Core = new(
        MuscleGroupName.Create("Core"),
        MuscleGroupDescription.Create("Abdominal and trunk stabilizer muscle group."));

    /// <summary>
    /// Full body muscle group.
    /// </summary>
    public static readonly MuscleGroup FullBody = new(
        MuscleGroupName.Create("Full Body"),
        MuscleGroupDescription.Create("Composite muscle group used when an exercise targets multiple primary regions."));

    private static readonly IReadOnlyList<MuscleGroup> _all =
    [
        Chest,
        Back,
        Shoulders,
        Biceps,
        Triceps,
        Forearms,
        Quadriceps,
        Hamstrings,
        Glutes,
        Calves,
        Core,
        FullBody
    ];

    private MuscleGroup(
        MuscleGroupName name,
        MuscleGroupDescription description,
        ImageData? thumbnail = null)
    {
        Name = Guard.AgainstNull(name, nameof(name));
        Description = Guard.AgainstNull(description, nameof(description));
        Thumbnail = thumbnail;
    }

    /// <summary>
    /// Gets the muscle group name.
    /// </summary>
    public MuscleGroupName Name { get; }

    /// <summary>
    /// Gets the muscle group description.
    /// </summary>
    public MuscleGroupDescription Description { get; }

    /// <summary>
    /// Gets optional thumbnail image data.
    /// </summary>
    public ImageData? Thumbnail { get; }

    /// <summary>
    /// Gets all predefined muscle group instances.
    /// </summary>
    public static IReadOnlyList<MuscleGroup> All => _all;

    /// <summary>
    /// Gets the components used for value equality.
    /// </summary>
    private IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
        yield return Description;
        yield return Thumbnail;
    }

    /// <inheritdoc />
    public bool Equals(MuscleGroup? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null)
        {
            return false;
        }

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is MuscleGroup other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        foreach (var component in GetEqualityComponents())
        {
            hashCode.Add(component);
        }

        return hashCode.ToHashCode();
    }
}
