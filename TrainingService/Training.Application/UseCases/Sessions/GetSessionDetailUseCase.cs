using Training.Application.DTOs;
using Training.Application.Interfaces;
using Training.Domain.Sessions;

namespace Training.Application.UseCases.Sessions;

public sealed class GetSessionDetailUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public GetSessionDetailUseCase(ISessionRepository sessionRepository, ICurrentUserAccessor currentUserAccessor)
    {
        _sessionRepository = sessionRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<SessionDetailResponse?> ExecuteAsync(Guid id, CancellationToken cancellationToken)
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

        var exercises = session.Exercises
            .OrderBy(x => x.Order.Value)
            .Select(x => new SessionExerciseDetailResponse(
                x.Id,
                x.ExerciseId,
                x.ExerciseNameSnapshot.Value,
                x.Order.Value,
                x.LoggedSets
                    .OrderBy(ls => ls.Order.Value)
                    .Select(ls => new LoggedSetResponse(
                        ls.Order.Value,
                        ls.Rpe.ToString(),
                        ls.Repetitions,
                        ls.Weight?.Kilograms,
                        ls.Duration is null ? null : (int)ls.Duration.Value.TotalSeconds,
                        ls.Distance?.Meters,
                        ls.HeartRate?.BeatsPerMinute,
                        ls.Note?.Value))
                    .ToArray()))
            .ToArray();

        return new SessionDetailResponse(
            session.Id,
            session.RoutineId,
            session.StartedAtUtc,
            session.EndedAtUtc,
            session.IsEnded,
            session.Note?.Value,
            exercises);
    }
}
