using Training.Application.Interfaces;
using Training.Domain.Sessions;

namespace Training.Application.UseCases.Sessions;

public sealed class UpdateSessionNoteUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public UpdateSessionNoteUseCase(ISessionRepository sessionRepository, ICurrentUserAccessor currentUserAccessor)
    {
        _sessionRepository = sessionRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<bool> ExecuteAsync(Guid id, string note, CancellationToken cancellationToken)
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

        session.UpdateNote(SessionNote.Create(note));
        await _sessionRepository.UpdateAsync(session, cancellationToken);
        return true;
    }
}
