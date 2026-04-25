using Training.Domain.Exercises;

namespace Training.Api.Tests;

public sealed class ExerciseAggregateTests
{
    [Fact]
    public void Create_WithValidInput_CreatesExercise()
    {
        var exercise = CreateExercise();

        Assert.Equal(ExerciseName.Create("Bench Press"), exercise.Name);
        Assert.Equal(ExerciseDescription.Create("Barbell press movement."), exercise.Description);
        Assert.Equal(DifficultyLevel.Intermediate, exercise.DifficultyLevel);
        Assert.Equal(ExerciseCategory.Strength, exercise.Category);
        Assert.Equal(MuscleGroup.Chest, exercise.PrimaryMuscleGroup);
        Assert.Single(exercise.RequiredEquipment);
    }

    [Fact]
    public void Create_WithEmptyRequiredEquipment_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Exercise.Create(
                ExerciseName.Create("Bench Press"),
                ExerciseDescription.Create("Barbell press movement."),
                DifficultyLevel.Intermediate,
                ExerciseCategory.Strength,
                MuscleGroup.Chest,
                Hyperlink.Create("https://example.com/video"),
                ImageData.Create([1, 2, 3], ImageFormat.Png),
                []));

        Assert.Equal("requiredEquipment", ex.ParamName);
    }

    [Fact]
    public void Create_WithDuplicateEquipment_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Exercise.Create(
                ExerciseName.Create("Bench Press"),
                ExerciseDescription.Create("Barbell press movement."),
                DifficultyLevel.Intermediate,
                ExerciseCategory.Strength,
                MuscleGroup.Chest,
                Hyperlink.Create("https://example.com/video"),
                ImageData.Create([1, 2, 3], ImageFormat.Png),
                [EquipmentType.Barbell, EquipmentType.Barbell]));

        Assert.Equal("requiredEquipment", ex.ParamName);
    }

    [Fact]
    public void Create_WithAliasEqualToCanonicalName_ThrowsArgumentException()
    {
        var canonical = ExerciseName.Create("Bench Press");

        var ex = Assert.Throws<ArgumentException>(() =>
            Exercise.Create(
                canonical,
                ExerciseDescription.Create("Barbell press movement."),
                DifficultyLevel.Intermediate,
                ExerciseCategory.Strength,
                MuscleGroup.Chest,
                Hyperlink.Create("https://example.com/video"),
                ImageData.Create([1, 2, 3], ImageFormat.Png),
                [EquipmentType.Barbell],
                aliases: [ExerciseName.Create("bench press")]));

        Assert.Equal("aliases", ex.ParamName);
    }

    [Fact]
    public void Create_WithPrimaryAlsoInSecondary_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Exercise.Create(
                ExerciseName.Create("Bench Press"),
                ExerciseDescription.Create("Barbell press movement."),
                DifficultyLevel.Intermediate,
                ExerciseCategory.Strength,
                MuscleGroup.Chest,
                Hyperlink.Create("https://example.com/video"),
                ImageData.Create([1, 2, 3], ImageFormat.Png),
                [EquipmentType.Barbell],
                secondaryMuscleGroups: [MuscleGroup.Chest]));

        Assert.Equal("Primary muscle group cannot appear in secondary muscle groups.", ex.Message);
    }

    [Fact]
    public void Create_WithDuplicateSecondaryMuscleGroups_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Exercise.Create(
                ExerciseName.Create("Bench Press"),
                ExerciseDescription.Create("Barbell press movement."),
                DifficultyLevel.Intermediate,
                ExerciseCategory.Strength,
                MuscleGroup.Chest,
                Hyperlink.Create("https://example.com/video"),
                ImageData.Create([1, 2, 3], ImageFormat.Png),
                [EquipmentType.Barbell],
                secondaryMuscleGroups: [MuscleGroup.Shoulders, MuscleGroup.Shoulders]));

        Assert.Equal("secondaryMuscleGroups", ex.ParamName);
    }

    private static Exercise CreateExercise()
    {
        return Exercise.Create(
            ExerciseName.Create("Bench Press"),
            ExerciseDescription.Create("Barbell press movement."),
            DifficultyLevel.Intermediate,
            ExerciseCategory.Strength,
            MuscleGroup.Chest,
            Hyperlink.Create("https://example.com/video"),
            ImageData.Create([1, 2, 3], ImageFormat.Png),
            [EquipmentType.Barbell],
            aliases: [ExerciseName.Create("Barbell Bench Press")],
            secondaryMuscleGroups: [MuscleGroup.Shoulders, MuscleGroup.Triceps]);
    }
}
