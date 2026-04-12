using Training.Application.DTOs;

namespace Training.Application.UseCases;

public sealed class GetTrainingStatusUseCase
{
    public TrainingStatusDto Execute()
    {
        return new TrainingStatusDto("Training.Api", "ok");
    }
}
