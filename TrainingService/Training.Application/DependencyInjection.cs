using Microsoft.Extensions.DependencyInjection;
using Training.Application.Interfaces;
using Training.Application.Services;
using Training.Application.UseCases;

namespace Training.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITrainingService, TrainingService>();
        services.AddScoped<GetTrainingStatusUseCase>();
        return services;
    }
}
