using Training.Domain.Common;
using System.Collections.ObjectModel;

namespace Training.Domain.Exercises;

/// <summary>
/// Represents an exercise catalog entry aggregate root.
/// </summary>
public sealed class Exercise
{
    private readonly ReadOnlyCollection<MuscleGroup> _secondaryMuscleGroups;
    private readonly ReadOnlyCollection<EquipmentType> _requiredEquipment;
    private readonly ReadOnlyCollection<ExerciseName> _aliases;

    private Exercise(
        Guid id,
        ExerciseName name,
        ExerciseDescription description,
        DifficultyLevel difficultyLevel,
        ExerciseCategory category,
        Hyperlink instructionVideoUrl,
        ImageData thumbnail,
        MuscleGroup primaryMuscleGroup,
        IEnumerable<EquipmentType> requiredEquipment,
        IEnumerable<ExerciseName>? aliases,
        IEnumerable<MuscleGroup>? secondaryMuscleGroups)
    {
        Id = Guard.AgainstEmptyGuid(id, nameof(id));
        Name = Guard.AgainstNull(name, nameof(name));
        Description = Guard.AgainstNull(description, nameof(description));
        DifficultyLevel = difficultyLevel;
        Category = category;
        InstructionVideoUrl = Guard.AgainstNull(instructionVideoUrl, nameof(instructionVideoUrl));
        EnsureInstructionVideoUrlFormat(InstructionVideoUrl, nameof(instructionVideoUrl));
        Thumbnail = Guard.AgainstNull(thumbnail, nameof(thumbnail));
        EnsureThumbnailFormat(Thumbnail, nameof(thumbnail));
        PrimaryMuscleGroup = Guard.AgainstNull(primaryMuscleGroup, nameof(primaryMuscleGroup));
        EnsurePredefinedMuscleGroup(PrimaryMuscleGroup, nameof(primaryMuscleGroup));

        ArgumentNullException.ThrowIfNull(requiredEquipment);
        var requiredEquipmentList = requiredEquipment.ToList();
        EnsureRequiredEquipmentNotEmpty(requiredEquipmentList);
        EnsureNoDuplicates(requiredEquipmentList, nameof(requiredEquipment), "Required equipment must not contain duplicates.");

        var aliasesList = aliases?.Select(x => Guard.AgainstNull(x, nameof(aliases))).ToList() ?? [];
        EnsureNoDuplicates(aliasesList, nameof(aliases), "Aliases must not contain duplicates.");

        var secondaryMuscleGroupsList = secondaryMuscleGroups?.Select(x => Guard.AgainstNull(x, nameof(secondaryMuscleGroups))).ToList() ?? [];
        EnsureNoDuplicates(secondaryMuscleGroupsList, nameof(secondaryMuscleGroups), "Secondary muscle groups must not contain duplicates.");
        EnsureOnlyPredefinedMuscleGroups(secondaryMuscleGroupsList, nameof(secondaryMuscleGroups));

        _requiredEquipment = new ReadOnlyCollection<EquipmentType>(requiredEquipmentList);
        _aliases = new ReadOnlyCollection<ExerciseName>(aliasesList);
        _secondaryMuscleGroups = new ReadOnlyCollection<MuscleGroup>(secondaryMuscleGroupsList);

        EnsureAliasConsistency(Name, _aliases);
        EnsureMuscleGroupConsistency(PrimaryMuscleGroup, _secondaryMuscleGroups);
    }

    /// <summary>
    /// Gets the exercise identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the exercise name.
    /// </summary>
    public ExerciseName Name { get; }

    /// <summary>
    /// Gets the exercise description.
    /// </summary>
    public ExerciseDescription Description { get; }

    /// <summary>
    /// Gets the exercise difficulty level.
    /// </summary>
    public DifficultyLevel DifficultyLevel { get; }

    /// <summary>
    /// Gets the exercise category.
    /// </summary>
    public ExerciseCategory Category { get; }

    /// <summary>
    /// Gets the instruction video URL.
    /// </summary>
    public Hyperlink InstructionVideoUrl { get; }

    /// <summary>
    /// Gets the exercise thumbnail image.
    /// </summary>
    public ImageData Thumbnail { get; }

    /// <summary>
    /// Gets the primary targeted muscle group.
    /// </summary>
    public MuscleGroup PrimaryMuscleGroup { get; }

    /// <summary>
    /// Gets the secondary targeted muscle groups.
    /// </summary>
    public ReadOnlyCollection<MuscleGroup> SecondaryMuscleGroups => _secondaryMuscleGroups;

    /// <summary>
    /// Gets the required equipment collection.
    /// </summary>
    public ReadOnlyCollection<EquipmentType> RequiredEquipment => _requiredEquipment;

    /// <summary>
    /// Gets the exercise aliases.
    /// </summary>
    public ReadOnlyCollection<ExerciseName> Aliases => _aliases;

    /// <summary>
    /// Creates a new <see cref="Exercise"/>.
    /// </summary>
    public static Exercise Create(
        ExerciseName name,
        ExerciseDescription description,
        DifficultyLevel difficultyLevel,
        ExerciseCategory category,
        MuscleGroup primaryMuscleGroup,
        Hyperlink instructionVideoUrl,
        ImageData thumbnail,
        IEnumerable<EquipmentType> requiredEquipment,
        IEnumerable<ExerciseName>? aliases = null,
        IEnumerable<MuscleGroup>? secondaryMuscleGroups = null)
    {
        return new Exercise(
            Guid.NewGuid(),
            name,
            description,
            difficultyLevel,
            category,
            instructionVideoUrl,
            thumbnail,
            primaryMuscleGroup,
            requiredEquipment,
            aliases,
            secondaryMuscleGroups);
    }

    /// <inheritdoc />
    public override string ToString() => Name.ToString();

    private static void EnsureMuscleGroupConsistency(MuscleGroup primaryMuscleGroup, IReadOnlyCollection<MuscleGroup> secondaryMuscleGroups)
    {
        if (secondaryMuscleGroups.Any(x => x.Equals(primaryMuscleGroup)))
        {
            throw new ArgumentException("Primary muscle group cannot appear in secondary muscle groups.");
        }
    }

    private static void EnsureRequiredEquipmentNotEmpty(IReadOnlyCollection<EquipmentType> requiredEquipment)
    {
        if (requiredEquipment.Count == 0)
        {
            throw new ArgumentException("At least one required equipment value must be provided.", nameof(requiredEquipment));
        }
    }

    private static void EnsureAliasConsistency(ExerciseName canonicalName, IReadOnlyCollection<ExerciseName> aliases)
    {
        if (aliases.Any(alias => alias.Equals(canonicalName)))
        {
            throw new ArgumentException("Aliases cannot include the canonical exercise name.", nameof(aliases));
        }
    }

    private static void EnsurePredefinedMuscleGroup(MuscleGroup muscleGroup, string paramName)
    {
        if (!MuscleGroup.All.Any(known => known.Equals(muscleGroup)))
        {
            throw new ArgumentException("Only predefined muscle groups are allowed.", paramName);
        }
    }

    private static void EnsureInstructionVideoUrlFormat(Hyperlink instructionVideoUrl, string paramName)
    {
        var uri = instructionVideoUrl.Value;

        if (!uri.IsAbsoluteUri || uri.Scheme is not ("http" or "https"))
        {
            throw new ArgumentException("Instruction video URL must be an absolute HTTP or HTTPS URL.", paramName);
        }
    }

    private static void EnsureThumbnailFormat(ImageData thumbnail, string paramName)
    {
        if (!Enum.IsDefined(thumbnail.Format))
        {
            throw new ArgumentException("Thumbnail format must be a valid image format.", paramName);
        }
    }

    private static void EnsureOnlyPredefinedMuscleGroups(IEnumerable<MuscleGroup> muscleGroups, string paramName)
    {
        foreach (var muscleGroup in muscleGroups)
        {
            EnsurePredefinedMuscleGroup(muscleGroup, paramName);
        }
    }

    private static void EnsureNoDuplicates<T>(IReadOnlyCollection<T> values, string paramName, string message)
        where T : notnull
    {
        if (values.Count != values.Distinct().Count())
        {
            throw new ArgumentException(message, paramName);
        }
    }
}
