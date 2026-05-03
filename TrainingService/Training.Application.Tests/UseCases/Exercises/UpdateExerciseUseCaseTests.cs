using Training.Application.UseCases.Exercises;
using Training.Domain.Exercises;

namespace Training.Application.Tests.UseCases.Exercises;

public sealed class UpdateExerciseUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenExerciseMissing_ReturnsNull()
    {
        var repository = new FakeExerciseRepository(null);
        var useCase = new UpdateExerciseUseCase(repository);

        var result = await useCase.ExecuteAsync(
            Guid.NewGuid(),
            "Bench Press",
            "Press",
            DifficultyLevel.Beginner,
            ExerciseCategory.Strength,
            MovementPattern.Push,
            MuscleGroup.Chest,
            [EquipmentType.Barbell],
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExerciseExists_ReturnsUpdatedResponse()
    {
        var existing = Exercise.Create(
            ExerciseName.Create("Bench Press"),
            ExerciseDescription.Create("Press"),
            DifficultyLevel.Beginner,
            ExerciseCategory.Strength,
            MovementPattern.Push,
            MuscleGroup.Chest,
            Hyperlink.Create("https://example.com/instruction"),
            ImageData.Create([1], ImageFormat.Png),
            [EquipmentType.Barbell]);

        var repository = new FakeExerciseRepository(existing);
        var useCase = new UpdateExerciseUseCase(repository);

        var result = await useCase.ExecuteAsync(
            existing.Id,
            "Bench Press Updated",
            "Updated description",
            DifficultyLevel.Intermediate,
            ExerciseCategory.Strength,
            MovementPattern.Push,
            MuscleGroup.Chest,
            [EquipmentType.Dumbbell],
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Bench Press Updated", result.Name);
        Assert.Equal("Intermediate", result.DifficultyLevel);
        Assert.Equal(["Dumbbell"], result.RequiredEquipment);
    }

    private sealed class FakeExerciseRepository : IExerciseRepository
    {
        private Exercise? _exercise;

        public FakeExerciseRepository(Exercise? exercise)
        {
            _exercise = exercise;
        }

        public Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_exercise);
        }

        public Task<IReadOnlyList<Exercise>> ListAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Exercise>>(_exercise is null ? [] : [_exercise]);
        }

        public Task AddAsync(Exercise exercise, CancellationToken cancellationToken)
        {
            _exercise = exercise;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Exercise exercise, CancellationToken cancellationToken)
        {
            _exercise = exercise;
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_exercise is not null && _exercise.Id == id);
        }
    }
}
