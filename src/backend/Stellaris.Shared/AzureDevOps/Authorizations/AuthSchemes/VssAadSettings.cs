namespace Stellaris.Shared.AzureDevOps.Authorizations.AuthSchemes
{
    // Reference: https://learn.microsoft.com/en-us/azure/devops/organizations/accounts/manage-personal-access-tokens-via-api?view=azure-devops#configure-a-quickstart-application
    public class VssAadSettings
    {
        public static string[] DefaultScopes = new string[] { "499b84ac-1321-427f-aa17-267ca6975798/.default" };
    }
}
