using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Training.Application.Interfaces;
using Training.Domain.Exercises;
using Training.Domain.Routines;
using Training.Domain.Sessions;
using Training.Infrastructure.Persistence;
using Training.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Training.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TrainingDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("TrainingDb")
                ?? "Server=(localdb)\\mssqllocaldb;Database=TrainingServiceDb;Trusted_Connection=True;MultipleActiveResultSets=true;";

            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
        });

        services.AddScoped<ITrainingRepository, TrainingRepository>();
        services.AddScoped<IExerciseRepository, ExerciseRepository>();
        services.AddScoped<IRoutineRepository, RoutineRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        return services;
    }
}
