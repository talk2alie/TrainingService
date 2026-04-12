namespace Training.Infrastructure.Repositories;

public sealed class TrainingRepository
{
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
