using Training.Application.Interfaces;

namespace Training.Infrastructure.Repositories;

public sealed class TrainingRepository : ITrainingRepository
{
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
