using Autofac.Extensions.DependencyInjection;
using FastEndpoints;
using FastEndpoints.Swagger;
using Invoria.Api;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.BuildingBlocks.Infrastructure.Extensions;
using Serilog;
using Serilog.Events;


Log.Logger = new LoggerConfiguration()
#if DEBUG
    .MinimumLevel.Debug()
#else
    .MinimumLevel.Information()
#endif
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Async(c => c.Console())
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.InstallModule<ApiModuleInstaller>();



builder.Host
    .UseSerilog()
    .UseServiceProviderFactory(new AutofacServiceProviderFactory());



var app = builder.Build();


if (builder.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.UseHttpsRedirection()
     .UseCors(bld =>
        bld
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
    )
    .UseExceptionHandler()
    .UseRouting()

    .UseEndpoints(endpoint =>
    {
        endpoint.MapFastEndpoints(e =>
        {
            e.Endpoints.ShortNames = true;

            e.Errors.ResponseBuilder = (failures, ctx, statusCode) =>
            {
                var errors = failures
                    .GroupBy(f => f.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).Distinct().ToArray());

                var problem = new ApiProblemDetails
                {
                    Type = $"https://httpstatuses.io/{statusCode}",
                    Title = "Validation failed.",
                    Status = statusCode,
                    Instance = ctx.Request.Path,
                    CorrelationId = ctx.TraceIdentifier,
                    ErrorCode = "validation_error",
                    Errors = errors
                };

                return Envelope.Failure(problem);
            };
        });
    });

await app.RunModulesBootstrapperAsync();

app.Run();

public partial class Program { }
