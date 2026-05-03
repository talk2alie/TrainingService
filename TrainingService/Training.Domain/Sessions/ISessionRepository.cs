namespace Training.Domain.Sessions;

public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Session>> ListByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken);
    Task AddAsync(Session session, CancellationToken cancellationToken);
    Task UpdateAsync(Session session, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
}
