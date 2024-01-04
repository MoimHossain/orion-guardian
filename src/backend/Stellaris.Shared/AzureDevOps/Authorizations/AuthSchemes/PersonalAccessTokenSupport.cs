

using Stellaris.Shared.Config;
using System.Text;

namespace Stellaris.Shared.AzureDevOps.Authorizations.AuthSchemes
{
    public class PersonalAccessTokenSupport
    {
        private readonly AzureDevOpsClientConfig config;

        public PersonalAccessTokenSupport(AzureDevOpsClientConfig config)
        {
            this.config = config;
        }

        public bool IsConfigured()
        {
            return config.usePat && !string.IsNullOrEmpty(config.Pat);
        }

        public (string, string) GetCredentials()
        {
            var base64Credential = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", config.Pat)));
            var scheme = "Basic";
            return (scheme, base64Credential);
        }
    }
}
