
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Stellaris.Shared.Config;
using Stellaris.Shared.Storage.Abstracts;

namespace Stellaris.Shared.Storage
{
    public class GeneralCosmosClient(CosmosDbConfig cosmosDbConfig, ILogger<GeneralCosmosClient> logger) : CosmosClientBase(cosmosDbConfig, logger)
    {
        protected override string ContainerId => throw new NotImplementedException();

        public async Task<Database> InitializeDatabaseAsync()
        {
            var client = GetCosmosClient();
            DatabaseResponse response = await client.CreateDatabaseIfNotExistsAsync( id: Config.DatabaseId);

            Logger.LogInformation($"Created database {Config.DatabaseId} with request charge of {response.RequestCharge}.");

            var database = response.Database;

            foreach (var (containerId, partitionKeyPath) in ContainerConstants.CONTAINERS)
            {
                await database.CreateContainerIfNotExistsAsync(
                    id: containerId,
                    partitionKeyPath: partitionKeyPath
                );
                Logger.LogInformation($"Created container {containerId} with partition key {partitionKeyPath}.");
            }
            return database;
        }
    }
}
