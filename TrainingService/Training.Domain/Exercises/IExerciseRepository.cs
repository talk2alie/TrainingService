namespace Training.Domain.Exercises;

public interface IExerciseRepository
{
    Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Exercise exercise, CancellationToken cancellationToken);
    Task UpdateAsync(Exercise exercise, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
}
