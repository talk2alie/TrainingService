using Training.Application.Interfaces;
using Training.Domain.Sessions;

namespace Training.Application.UseCases.Sessions;

public sealed class EndSessionUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public EndSessionUseCase(ISessionRepository sessionRepository, ICurrentUserAccessor currentUserAccessor)
    {
        _sessionRepository = sessionRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<bool> ExecuteAsync(Guid id, DateTimeOffset endedAtUtc, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(id, cancellationToken);
        if (session is null)
        {
            return false;
        }

        if (session.OwnerUserId != _currentUserAccessor.GetRequiredUserId())
        {
            return false;
        }

        session.EndSession(endedAtUtc);
        await _sessionRepository.UpdateAsync(session, cancellationToken);
        return true;
    }
}
