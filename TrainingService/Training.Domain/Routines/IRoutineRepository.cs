namespace Training.Domain.Routines;

public interface IRoutineRepository
{
    Task<Routine?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Routine routine, CancellationToken cancellationToken);
    Task UpdateAsync(Routine routine, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
}
