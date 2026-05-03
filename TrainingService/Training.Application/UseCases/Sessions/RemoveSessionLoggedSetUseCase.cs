using Training.Application.Interfaces;
using Training.Domain.Common;
using Training.Domain.Sessions;

namespace Training.Application.UseCases.Sessions;

public sealed class RemoveSessionLoggedSetUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public RemoveSessionLoggedSetUseCase(ISessionRepository sessionRepository, ICurrentUserAccessor currentUserAccessor)
    {
        _sessionRepository = sessionRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<bool> ExecuteAsync(
        Guid sessionId,
        Guid sessionExerciseId,
        int setOrder,
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

        session.RemoveLoggedSet(sessionExerciseId, Order.Create(setOrder));
        await _sessionRepository.UpdateAsync(session, cancellationToken);
        return true;
    }
}
