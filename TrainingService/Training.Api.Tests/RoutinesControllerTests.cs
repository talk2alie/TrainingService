using Microsoft.AspNetCore.Mvc;
using Training.Api.Controllers;
using Training.Api.Requests;
using Training.Application.Interfaces;
using Training.Application.UseCases.Routines;
using Training.Domain.Exercises;
using Training.Domain.Routines;

namespace Training.Api.Tests;

public sealed class RoutinesControllerTests
{
    [Fact]
    public async Task Create_ReturnsCreated()
    {
        var controller = new RoutinesController();
        var useCase = new CreateRoutineUseCase(new FakeRoutineRepository(), new FakeUserAccessor());

        var result = await controller.Create(new CreateRoutineRequest("Push", "Desc", DifficultyLevel.Beginner), useCase, CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task List_ReturnsOk()
    {
        var controller = new RoutinesController();
        var useCase = new ListRoutinesUseCase(new FakeRoutineRepository(), new FakeUserAccessor());

        var result = await controller.List(useCase, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Archive_WhenNotFound_ReturnsNotFound()
    {
        var controller = new RoutinesController();
        var useCase = new ArchiveRoutineUseCase(new FakeRoutineRepository(), new FakeUserAccessor());

        var result = await controller.Archive(Guid.NewGuid(), useCase, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    private sealed class FakeRoutineRepository : IRoutineRepository
    {
        public Task<Routine?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<Routine?>(null);
        public Task<IReadOnlyList<Routine>> ListByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Routine>>([]);
        public Task AddAsync(Routine routine, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task UpdateAsync(Routine routine, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(false);
    }

    private sealed class FakeUserAccessor : ICurrentUserAccessor
    {
        public Guid GetRequiredUserId() => Guid.NewGuid();
    }
}
