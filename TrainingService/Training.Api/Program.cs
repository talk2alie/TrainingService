
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using Training.Api.Middleware;
using Training.Api.Services;
using Training.Api.Swagger;
using Training.Application;
using Training.Domain;
using Training.Infrastructure;

namespace Training.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.WithProperty("CorrelationId", "n/a")
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
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<Training.Application.Interfaces.ICurrentUserAccessor, CurrentUserAccessor>();

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = builder.Configuration["Authentication:Authority"];
                options.Audience = builder.Configuration["Authentication:Audience"];
                options.RequireHttpsMetadata = true;
            });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("Training.Api"))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter();
            });

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

        if (app.Logger.IsEnabled(LogLevel.Information))
        {
            app.Logger.LogInformation("Training API is starting in {Environment}.", app.Environment.EnvironmentName);
        }

        // Configure the HTTP request pipeline.
        var swaggerEnabled = app.Configuration.GetValue<bool>("Swagger:Enabled");
        if (swaggerEnabled)
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

        app.UseCorrelationIdMiddleware();
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
            };
        });
        app.UseGlobalExceptionMiddleware();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();
        app.MapHealthChecks("/health");

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            if (app.Logger.IsEnabled(LogLevel.Information))
            {
                app.Logger.LogInformation("Training API started successfully.");
            }
        });

        app.Run();
    }
}
