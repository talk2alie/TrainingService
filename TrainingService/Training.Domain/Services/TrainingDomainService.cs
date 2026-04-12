namespace Training.Domain.Services;

public sealed class TrainingDomainService : ITrainingDomainService
{
    public string GetServiceName()
    {
        return "Training.Api";
    }
}
