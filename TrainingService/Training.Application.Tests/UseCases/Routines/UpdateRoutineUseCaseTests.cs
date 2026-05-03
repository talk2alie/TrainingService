using Training.Application.Interfaces;
using Training.Application.UseCases.Routines;
using Training.Domain.Common;
using Training.Domain.Exercises;
using Training.Domain.Routines;

namespace Training.Application.Tests.UseCases.Routines;

public sealed class UpdateRoutineUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenRoutineMissing_ReturnsNull()
    {
        var useCase = new UpdateRoutineUseCase(new FakeRoutineRepository(null), new FakeCurrentUserAccessor(Guid.NewGuid()));

        var result = await useCase.ExecuteAsync(Guid.NewGuid(), "Name", "Desc", DifficultyLevel.Beginner, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRoutineExistsAndUserMatches_ReturnsUpdatedResponse()
    {
        var ownerId = Guid.NewGuid();
        var routine = Routine.Create(ownerId, RoutineName.Create("Old"), RoutineDescription.Create("Old Desc"), DifficultyLevel.Beginner, []);
        routine.AddExercise(Guid.NewGuid(), ExerciseName.Create("Bench Press"), [PlannedSet.Create(Order.Create(1), repetitions: 10, weight: Weight.Create(60))]);

        var useCase = new UpdateRoutineUseCase(new FakeRoutineRepository(routine), new FakeCurrentUserAccessor(ownerId));

        var result = await useCase.ExecuteAsync(routine.Id, "New", "New Desc", DifficultyLevel.Intermediate, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New", result!.Name);
        Assert.Equal("New Desc", result.Description);
        Assert.Equal(nameof(DifficultyLevel.Intermediate), result.DifficultyLevel);
    }

    private sealed class FakeCurrentUserAccessor : ICurrentUserAccessor
    {
        private readonly Guid _id;

        public FakeCurrentUserAccessor(Guid id)
        {
            _id = id;
        }

        public Guid GetRequiredUserId() => _id;
    }

    private sealed class FakeRoutineRepository : IRoutineRepository
    {
        private readonly Routine? _routine;

        public FakeRoutineRepository(Routine? routine)
        {
            _routine = routine;
        }

        public Task<Routine?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_routine);
        public Task<IReadOnlyList<Routine>> ListByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Routine>>([]);
        public Task AddAsync(Routine routine, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task UpdateAsync(Routine routine, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(false);
    }
}
