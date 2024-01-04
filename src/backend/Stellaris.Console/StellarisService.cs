

using Microsoft.Extensions.Hosting;
using Stellaris.Shared.Business;

namespace Stellaris.Console
{
    public class StellarisService(ChangeEventProcessingService changeEventProcessingService) : BackgroundService
    {   
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await changeEventProcessingService.StartAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
            await changeEventProcessingService.StopAsync();
        }
    }
}
