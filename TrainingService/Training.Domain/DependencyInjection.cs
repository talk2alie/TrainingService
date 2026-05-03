using Microsoft.Extensions.DependencyInjection;
using Training.Domain.Common;
using Training.Domain.Exercises;
using Training.Domain.Routines;
using Training.Domain.Sessions;
using Training.Domain.Services;

namespace Training.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<ITrainingDomainService, TrainingDomainService>();
        services.AddScoped<IAggregatePersistenceValidator, AggregatePersistenceValidator>();
        services.AddScoped<ISessionRoutineValidator, SessionRoutineValidator>();
        services.AddScoped<IExerciseReferenceValidator>(_ => new ExerciseReferenceValidator([]));
        services.AddScoped<IRoutineArchivalValidator>(_ => new RoutineArchivalValidator(new NoActiveSessionChecker()));
        return services;
    }

    private sealed class NoActiveSessionChecker : IActiveSessionChecker
    {
        public bool HasActiveSessions(Guid routineId)
        {
            return false;
        }
    }
}
