using Microsoft.Extensions.DependencyInjection;
using Training.Application.Interfaces;
using Training.Application.Services;
using Training.Application.UseCases;
using Training.Application.UseCases.Exercises;
using Training.Application.UseCases.Routines;
using Training.Application.UseCases.Sessions;

namespace Training.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITrainingService, TrainingService>();
        services.AddScoped<GetTrainingStatusUseCase>();
        services.AddScoped<CreateExerciseUseCase>();
        services.AddScoped<GetExerciseByIdUseCase>();
        services.AddScoped<ListExercisesUseCase>();
        services.AddScoped<UpdateExerciseUseCase>();
        services.AddScoped<CreateRoutineUseCase>();
        services.AddScoped<GetRoutineByIdUseCase>();
        services.AddScoped<ListRoutinesUseCase>();
        services.AddScoped<UpdateRoutineUseCase>();
        services.AddScoped<ArchiveRoutineUseCase>();
        services.AddScoped<StartSessionUseCase>();
        services.AddScoped<GetSessionByIdUseCase>();
        services.AddScoped<GetSessionDetailUseCase>();
        services.AddScoped<ListSessionsUseCase>();
        services.AddScoped<UpdateSessionNoteUseCase>();
        services.AddScoped<AddSessionLoggedSetUseCase>();
        services.AddScoped<RemoveSessionLoggedSetUseCase>();
        services.AddScoped<EndSessionUseCase>();
        return services;
    }
}
