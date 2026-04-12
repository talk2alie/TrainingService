
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Options;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using Training.Application;
using Training.Domain;
using Training.Infrastructure;
using Swashbuckle.AspNetCore.SwaggerGen;
using Training.Api.Middleware;
using Training.Api.Swagger;

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

        builder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        builder.Services.AddDomainServices();
        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices();

        builder.Services.AddControllers();
        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();
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
        builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks();

        var app = builder.Build();

        app.Logger.LogInformation("Training API is starting.");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            var apiVersionDescriptionProvider = app.DescribeApiVersions();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var description in apiVersionDescriptionProvider)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });
        }

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseGlobalExceptionMiddleware();

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();
        app.MapHealthChecks("/health");

        app.Lifetime.ApplicationStarted.Register(() =>
            app.Logger.LogInformation("Training API started successfully."));

        app.Run();
    }
}
