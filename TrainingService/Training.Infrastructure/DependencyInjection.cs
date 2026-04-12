using Microsoft.Extensions.DependencyInjection;
using Training.Application.Interfaces;
using Training.Infrastructure.Repositories;

namespace Training.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<ITrainingRepository, TrainingRepository>();
        return services;
    }
}
