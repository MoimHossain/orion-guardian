

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stellaris.Console;
using Stellaris.Shared.Config;
using Stellaris.Shared.Extensions;
using Stellaris.Shared.Storage;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);


builder.Services.AddServicesFromSharedLibrary();

builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.SingleLine = true;
    options.TimestampFormat = "hh:mm:ss ";
});
builder.Services.AddHostedService<StellarisService>();

IHost host = builder.Build();
await host.RunAsync();