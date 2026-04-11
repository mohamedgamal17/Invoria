using Autofac.Extensions.DependencyInjection;
using FastEndpoints;
using FastEndpoints.Swagger;
using Invoria.Api;
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
        e.Endpoints.ShortNames = true);
    });

await app.RunModulesBootstrapperAsync();

app.Run();

public partial class Program { }
