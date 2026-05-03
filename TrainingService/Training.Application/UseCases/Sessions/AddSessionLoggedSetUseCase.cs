using Training.Application.Interfaces;
using Training.Domain.Common;
using Training.Domain.Sessions;

namespace Training.Application.UseCases.Sessions;

public sealed class AddSessionLoggedSetUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public AddSessionLoggedSetUseCase(ISessionRepository sessionRepository, ICurrentUserAccessor currentUserAccessor)
    {
        _sessionRepository = sessionRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<bool> ExecuteAsync(
        Guid sessionId,
        Guid sessionExerciseId,
        int? repetitions,
        decimal? weightKg,
        int? durationSeconds,
        decimal? distanceMeters,
        RpeScale rpe,
        int? heartRateBpm,
        string? note,
        CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            return false;
        }

        if (session.OwnerUserId != _currentUserAccessor.GetRequiredUserId())
        {
            return false;
        }

        var exercise = session.Exercises.FirstOrDefault(x => x.Id == sessionExerciseId);
        if (exercise is null)
        {
            return false;
        }

        var nextOrder = exercise.LoggedSets.Count + 1;
        var loggedSet = LoggedSet.Create(
            Order.Create(nextOrder),
            rpe,
            repetitions,
            weightKg.HasValue ? Weight.Create(weightKg.Value) : null,
            durationSeconds.HasValue ? Duration.Create(TimeSpan.FromSeconds(durationSeconds.Value)) : null,
            distanceMeters.HasValue ? Distance.Create(distanceMeters.Value) : null,
            heartRateBpm.HasValue ? HeartRate.Create(heartRateBpm.Value) : null,
            string.IsNullOrWhiteSpace(note) ? null : LogNote.Create(note));

        session.AddLoggedSet(sessionExerciseId, loggedSet);
        await _sessionRepository.UpdateAsync(session, cancellationToken);
        return true;
    }
}
