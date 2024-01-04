

using Microsoft.AspNetCore.Http.Json;
using Stellaris.Shared.Extensions;
using Stellaris.Shared.Storage;
using Stellaris.WebApi.Middleware;
using Stellaris.WebApi.Routes;
using System.Text.Json.Serialization;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<MvcJsonOptions>(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.SingleLine = true;
    options.TimestampFormat = "hh:mm:ss ";
});
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddServicesFromSharedLibrary();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.MapApiRoutes();
app.UseDevOpsAccessTokenValidation();
app.UseAudits();

var generalCosmosClient = app.Services.GetRequiredService<GeneralCosmosClient>();
await generalCosmosClient.InitializeDatabaseAsync();

app.Run();