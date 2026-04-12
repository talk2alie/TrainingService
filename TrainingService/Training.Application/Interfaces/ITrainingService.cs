namespace Training.Application.Interfaces;

public interface ITrainingService
{
    Task<bool> GetStatusAsync(CancellationToken cancellationToken);
}
