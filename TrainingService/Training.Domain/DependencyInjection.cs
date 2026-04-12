using Microsoft.Extensions.DependencyInjection;
using Training.Domain.Services;

namespace Training.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<ITrainingDomainService, TrainingDomainService>();
        return services;
    }
}
