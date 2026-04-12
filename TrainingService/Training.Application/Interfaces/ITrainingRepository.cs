namespace Training.Application.Interfaces;

public interface ITrainingRepository
{
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken);
}
