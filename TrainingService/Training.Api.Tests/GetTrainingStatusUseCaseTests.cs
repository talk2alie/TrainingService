using Training.Application.UseCases;

namespace Training.Api.Tests;

public sealed class GetTrainingStatusUseCaseTests
{
    [Fact]
    public void Execute_ReturnsOkTrainingStatusResponse()
    {
        var useCase = new GetTrainingStatusUseCase();

        var response = useCase.Execute();

        Assert.Equal("Training.Api", response.Service);
        Assert.Equal("ok", response.Status);
    }
}
