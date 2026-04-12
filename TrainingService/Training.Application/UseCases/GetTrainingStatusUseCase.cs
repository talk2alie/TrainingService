using Training.Application.DTOs;

namespace Training.Application.UseCases;

public sealed class GetTrainingStatusUseCase
{
    public TrainingStatusResponse Execute()
    {
        return new TrainingStatusResponse("Training.Api", "ok");
    }
}
