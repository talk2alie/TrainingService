using Training.Application.DTOs;
using Training.Application.Interfaces;
using Training.Domain.Sessions;

namespace Training.Application.UseCases.Sessions;

public sealed class StartSessionUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public StartSessionUseCase(ISessionRepository sessionRepository, ICurrentUserAccessor currentUserAccessor)
    {
        _sessionRepository = sessionRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<SessionSummaryResponse> ExecuteAsync(Guid? routineId, CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.GetRequiredUserId();
        var session = Session.Start(userId, routineId, DateTimeOffset.UtcNow, []);
        await _sessionRepository.AddAsync(session, cancellationToken);

        return new SessionSummaryResponse(session.Id, session.RoutineId, session.StartedAtUtc, session.EndedAtUtc, session.IsEnded, []);
    }
}
