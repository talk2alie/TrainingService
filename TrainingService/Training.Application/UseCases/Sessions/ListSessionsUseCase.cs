using Training.Application.DTOs;
using Training.Application.Interfaces;
using Training.Domain.Sessions;

namespace Training.Application.UseCases.Sessions;

public sealed class ListSessionsUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public ListSessionsUseCase(ISessionRepository sessionRepository, ICurrentUserAccessor currentUserAccessor)
    {
        _sessionRepository = sessionRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<IReadOnlyList<SessionSummaryResponse>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var ownerUserId = _currentUserAccessor.GetRequiredUserId();
        var sessions = await _sessionRepository.ListByOwnerAsync(ownerUserId, cancellationToken);

        return sessions.Select(session => new SessionSummaryResponse(
            session.Id,
            session.RoutineId,
            session.StartedAtUtc,
            session.EndedAtUtc,
            session.IsEnded,
            session.Exercises.Select(x => new SessionExerciseResponse(x.Id, x.ExerciseId, x.ExerciseNameSnapshot.Value, x.Order.Value, x.LoggedSets.Count)).ToArray())).ToArray();
    }
}
