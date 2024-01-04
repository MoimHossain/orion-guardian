

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Stellaris.Shared.Config;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage.Abstracts;
using static Microsoft.Azure.Cosmos.Container;

namespace Stellaris.Shared.Storage
{
    public class FeedProcessorCosmosClient(CosmosDbConfig cosmosDbConfig, ILogger<FeedProcessorCosmosClient> logger)
        : CosmosClientBase(cosmosDbConfig, logger)
    {
        protected override string ContainerId => ContainerConstants.CONTAINERID_FEED_PROCESSOR_LEASES;

        public async Task<ChangeFeedProcessor> StartListeningForEventsAsync(ChangesHandler<ChangeEventEntity> changesHandler)
        {
            var cosmosClient = GetCosmosClient();            
            var leaseContainer = cosmosClient.GetContainer(Config.DatabaseId, ContainerConstants.CONTAINERID_FEED_PROCESSOR_LEASES);
            var sourceContainer = cosmosClient.GetContainer(Config.DatabaseId, ContainerConstants.CONTAINERID_CHANGE_EVENTS);
            
            Logger.LogInformation($"Starting Change Feed Processor on Container: {sourceContainer.Id}...");

            var changeFeedProcessor = sourceContainer
                .GetChangeFeedProcessorBuilder<ChangeEventEntity>(processorName: "changeFeedSample", onChangesDelegate: changesHandler)
                .WithInstanceName("consoleHost")
                .WithLeaseContainer(leaseContainer)
                .Build();

            await changeFeedProcessor.StartAsync();
            Logger.LogInformation($"Change Feed Processor started on Container: {sourceContainer.Id}.");
            return changeFeedProcessor;
        }

        public async Task StopListeningAsync(ChangeFeedProcessor changeFeedProcessor)
        {   
            await changeFeedProcessor.StopAsync();
            Logger.LogInformation($"Change Feed Processor stopped on Container: {ContainerConstants.CONTAINERID_CHANGE_EVENTS}.");
        }
    }
}
