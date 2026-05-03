using Training.Application.DTOs;
using Training.Application.Interfaces;
using Training.Domain.Sessions;

namespace Training.Application.UseCases.Sessions;

public sealed class GetSessionByIdUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public GetSessionByIdUseCase(ISessionRepository sessionRepository, ICurrentUserAccessor currentUserAccessor)
    {
        _sessionRepository = sessionRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<SessionSummaryResponse?> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(id, cancellationToken);
        if (session is null)
        {
            return null;
        }

        if (session.OwnerUserId != _currentUserAccessor.GetRequiredUserId())
        {
            return null;
        }

        return new SessionSummaryResponse(
            session.Id,
            session.RoutineId,
            session.StartedAtUtc,
            session.EndedAtUtc,
            session.IsEnded,
            session.Exercises.Select(x => new SessionExerciseResponse(x.Id, x.ExerciseId, x.ExerciseNameSnapshot.Value, x.Order.Value, x.LoggedSets.Count)).ToArray());
    }
}
