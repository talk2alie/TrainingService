using Training.Application.Interfaces;
using Training.Domain.Services;

namespace Training.Application.Services;

public sealed class TrainingService(ITrainingRepository trainingRepository, ITrainingDomainService trainingDomainService) : ITrainingService
{
    private readonly ITrainingRepository _trainingRepository = trainingRepository;
    private readonly ITrainingDomainService _trainingDomainService = trainingDomainService;

    public Task<bool> GetStatusAsync(CancellationToken cancellationToken)
    {
        _ = _trainingDomainService.GetServiceName();
        return _trainingRepository.IsAvailableAsync(cancellationToken);
    }
}
