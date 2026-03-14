using FastEndpoints;
using FastEndpoints.Swagger;
using Invoria.BuildingBlocks.Infrastructure.Extensions;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.SwaggerDocument(opt =>
{
    opt.AutoTagPathSegmentIndex = 0;

    opt.FlattenSchema = true;
});

builder.Services.AddModuleFastEndpoints();

builder.Services.AddSingleton<IResultToHttpMapper, DefaultResultToHttpMapper>();


builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

app.UseFastEndpoints();
app.UseOpenApi();

app.Run();
