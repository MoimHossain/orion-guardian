namespace Stellaris.Shared.Config
{
    public class ConfigHelper
    {
        public static AzureDevOpsClientConfig GetAzureDevOpsClientConfig()
        {
            var orgName = GetEnvironmentVariableAsString("AZURE_DEVOPS_ORGNAME", "");
            var useManagedIdentity = GetEnvironmentVariableAsBool("AZURE_DEVOPS_USE_MANAGED_IDENTITY", false);
            var clientIdOfManagedIdentity = GetEnvironmentVariableAsString("AZURE_DEVOPS_CLIENT_ID_OF_MANAGED_IDENTITY", "");
            var tenantIdOfManagedIdentity = GetEnvironmentVariableAsString("AZURE_DEVOPS_TENANT_ID_OF_MANAGED_IDENTITY", "");

            var useServicePrincipal = GetEnvironmentVariableAsBool("AZURE_DEVOPS_USE_SERVICE_PRINCIPAL", false);
            var clientIdOfServicePrincipal = GetEnvironmentVariableAsString("AZURE_DEVOPS_CLIENT_ID_OF_SERVICE_PRINCIPAL", "");
            var clientSecretOfServicePrincipal = GetEnvironmentVariableAsString("AZURE_DEVOPS_CLIENT_SECRET_OF_SERVICE_PRINCIPAL", "");
            var tenantIdOfServicePrincipal = GetEnvironmentVariableAsString("AZURE_DEVOPS_TENANT_ID_OF_SERVICE_PRINCIPAL", "");

            var usePat = GetEnvironmentVariableAsBool("AZURE_DEVOPS_USE_PAT", false);
            var pat = GetEnvironmentVariableAsString("AZURE_DEVOPS_PAT", "");

            return new AzureDevOpsClientConfig(
                orgName,
                useManagedIdentity, clientIdOfManagedIdentity, tenantIdOfManagedIdentity,
                useServicePrincipal, clientIdOfServicePrincipal, clientSecretOfServicePrincipal, tenantIdOfServicePrincipal,
                usePat, pat);
        }

        public static CosmosDbConfig GetCosmosDbConfig()
        {
            var connectionString = GetCosmosConnectionString();
            var databaseId = GetCosmosDatabaseId();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("connectionString is empty");
            }
            if (string.IsNullOrWhiteSpace(databaseId))
            {
                throw new Exception("databaseId is empty");
            }

            return new CosmosDbConfig(connectionString, databaseId);
        }

        private static string GetCosmosConnectionString()
        {
            return GetEnvironmentVariable("AZURE_COSMOS_CONNECTIONSTRING") ?? throw new Exception("AZURE_COSMOS_CONNECTIONSTRING not set");
        }

        private static string GetCosmosDatabaseId()
        {
            return GetEnvironmentVariable("AZURE_COSMOS_DATABASEID") ?? throw new Exception("AZURE_COSMOS_DATABASEID not set");
        }


        private static string? GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        private static string GetEnvironmentVariableAsString(string name, string defaultValue)
        {
            var value = GetEnvironmentVariable(name);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private static bool GetEnvironmentVariableAsBool(string name, bool defaultValue)
        {
            var value = GetEnvironmentVariable(name);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : bool.Parse(value);
        }
    }

    public record CosmosDbConfig(string ConnectionString, string DatabaseId);

    public record AzureDevOpsClientConfig(
        string orgName,
        bool useManagedIdentity, string clientIdOfManagedIdentity, string tenantIdOfManagedIdentity,
        bool useServicePrincipal, string clientIdOfServicePrincipal, string clientSecretOfServicePrincipal, string tenantIdOfServicePrincipal,
        bool usePat, string Pat);

    public static class AzureDevOpsClientConstants
    {
        public static class CoreAPI
        {
            public const string NAME = "AZUREDEVOPS_CORE_CLIENT";
            public const string URI = "https://dev.azure.com";            
        }

        public static class VSSPS_API
        {
            public const string NAME = "AZUREDEVOPS_VSSPS_CLIENT";
            public const string URI = "https://vssps.dev.azure.com";
        }

    }
}
