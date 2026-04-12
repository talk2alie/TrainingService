
using Serilog;
using Microsoft.AspNetCore.Mvc;
using Training.Api.Middleware;

namespace Training.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

        // Add services to the container.

        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });

        builder.Services.AddControllers();
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var details = context.ModelState
                    .Where(x => x.Value is { Errors.Count: > 0 })
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value!.Errors
                            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                                ? "Invalid value."
                                : error.ErrorMessage)
                            .ToArray());

                var envelope = new ErrorEnvelope(
                    Code: "validation_error",
                    Message: "One or more validation errors occurred.",
                    CorrelationId: context.HttpContext.TraceIdentifier,
                    Details: details);

                return new BadRequestObjectResult(envelope);
            };
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.Logger.LogInformation("Training API is starting.");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseGlobalExceptionMiddleware();

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Lifetime.ApplicationStarted.Register(() =>
            app.Logger.LogInformation("Training API started successfully."));

        app.Run();
    }
}
