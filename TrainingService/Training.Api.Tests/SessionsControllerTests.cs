using Microsoft.AspNetCore.Mvc;
using Training.Api.Controllers;
using Training.Api.Requests;
using Training.Application.Interfaces;
using Training.Application.UseCases.Sessions;
using Training.Domain.Sessions;

namespace Training.Api.Tests;

public sealed class SessionsControllerTests
{
    [Fact]
    public async Task Start_ReturnsCreated()
    {
        var controller = new SessionsController();
        var useCase = new StartSessionUseCase(new FakeSessionRepository(), new FakeUserAccessor());

        var result = await controller.Start(new StartSessionRequest(null), useCase, CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task List_ReturnsOk()
    {
        var controller = new SessionsController();
        var useCase = new ListSessionsUseCase(new FakeSessionRepository(), new FakeUserAccessor());

        var result = await controller.List(useCase, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task End_WhenNotFound_ReturnsNotFound()
    {
        var controller = new SessionsController();
        var useCase = new EndSessionUseCase(new FakeSessionRepository(), new FakeUserAccessor());

        var result = await controller.End(Guid.NewGuid(), new Training.Api.Requests.EndSessionRequest(DateTimeOffset.UtcNow), useCase, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    private sealed class FakeSessionRepository : ISessionRepository
    {
        public Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<Session?>(null);
        public Task<IReadOnlyList<Session>> ListByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Session>>([]);
        public Task AddAsync(Session session, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task UpdateAsync(Session session, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(false);
    }

    private sealed class FakeUserAccessor : ICurrentUserAccessor
    {
        public Guid GetRequiredUserId() => Guid.NewGuid();
    }
}
