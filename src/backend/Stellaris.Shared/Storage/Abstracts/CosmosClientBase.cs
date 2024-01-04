

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Stellaris.Shared.Config;

namespace Stellaris.Shared.Storage.Abstracts
{
    public abstract class CosmosClientBase
    {
        private readonly CosmosDbConfig cosmosDbConfig;
        private readonly ILogger logger;

        protected CosmosClientBase(CosmosDbConfig cosmosDbConfig, ILogger logger)
        {
            if (cosmosDbConfig is null)
                throw new ArgumentNullException(nameof(cosmosDbConfig));
            cosmosDbConfig.Deconstruct(out var connectionString, out var databaseId);
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new Exception("connectionString is empty");
            if (string.IsNullOrWhiteSpace(databaseId))
                throw new Exception("databaseId is empty");
            this.cosmosDbConfig = cosmosDbConfig;
            this.logger = logger;
        }

        protected virtual CosmosClient GetCosmosClient()
        {
            var client = new CosmosClient(cosmosDbConfig.ConnectionString, GetCosmosClientOptions());
            return client;
        }

        protected virtual CosmosClientOptions GetCosmosClientOptions()
        {
            return new CosmosClientOptions
            {
                ApplicationName = typeof(CosmosClientBase).Namespace,
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };
        }


        protected Container Container
        {
            get
            {
                return GetCosmosClient().GetContainer(Config.DatabaseId, ContainerId);
            }
        }

        protected ILogger Logger => logger;

        protected CosmosDbConfig Config => cosmosDbConfig;

        protected abstract string ContainerId { get; }
    }
}
