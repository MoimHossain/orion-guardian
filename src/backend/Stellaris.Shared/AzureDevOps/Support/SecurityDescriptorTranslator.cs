

using Stellaris.Shared.AzureDevOps.DTOs;
using Stellaris.Shared.Config;
using Stellaris.Shared.Schemas;

namespace Stellaris.Shared.AzureDevOps.Support
{
    public class SecurityDescriptorTranslator
    {
        private readonly List<string> inputDescriptors;
        private readonly DevOpsClient devOpsClient;
        private readonly AzureDevOpsClientConfig azureDevOpsClientConfig;
        private List<AzDoTranslatedIdentityDescriptor> translatedDescriptors;

        private SecurityDescriptorTranslator(
            IEnumerable<string> inputDescriptors,
            DevOpsClient devOpsClient,
            AzureDevOpsClientConfig azureDevOpsClientConfig)
        {
            this.inputDescriptors = new List<string>(inputDescriptors);
            this.devOpsClient = devOpsClient;
            this.azureDevOpsClientConfig = azureDevOpsClientConfig;
            translatedDescriptors = [];
        }

        public string Translate(string subjectDescriptor)
        {
            var translatedDescriptor = translatedDescriptors
                .FirstOrDefault(td => td.SubjectDescriptor.Equals(subjectDescriptor, StringComparison.OrdinalIgnoreCase));
            if (translatedDescriptor != null)
            {
                return translatedDescriptor.Descriptor;
            }
            
            return string.Empty;
        }

        private async Task TranslateAsync()
        {
            if(inputDescriptors != null && inputDescriptors.Any())
            {
                var commaSepSDs = string.Join(",", inputDescriptors);
                var translatedDescriptors = await devOpsClient.TranslateDescriptorsAsync(azureDevOpsClientConfig.orgName, commaSepSDs);
                this.translatedDescriptors.AddRange(translatedDescriptors);
            }            
        }

        public async static Task<SecurityDescriptorTranslator> FromAsync(
            IEnumerable<CustomRoleAssignmentEntity> roleAssignments,
            DevOpsClient devOpsClient, AzureDevOpsClientConfig azureDevOpsClientConfig)
        {
            ArgumentNullException.ThrowIfNull(roleAssignments, nameof(roleAssignments));
            ArgumentNullException.ThrowIfNull(devOpsClient, nameof(devOpsClient));

            var inputDescriptors = new List<string>();
            foreach(var roleAssignment in roleAssignments)
            {
                if(roleAssignment.Identity != null)
                {
                    inputDescriptors.Add(roleAssignment.Identity.SubjectDescriptor);
                }
            }

            var translator = new SecurityDescriptorTranslator(inputDescriptors.Distinct(), devOpsClient, azureDevOpsClientConfig);
            await translator.TranslateAsync();

            return translator;
        }

        public async Task IncludeAsync(IEnumerable<string> subjectDescriptorsToTranslate)
        {
            ArgumentNullException.ThrowIfNull(subjectDescriptorsToTranslate, nameof(subjectDescriptorsToTranslate));

            var unknownDescriptors = subjectDescriptorsToTranslate.Where(sd => !inputDescriptors.Contains(sd));

            if(unknownDescriptors != null && unknownDescriptors.Any())
            {
                var commaSepSDs = string.Join(",", unknownDescriptors);
                var translatedDescriptors = await devOpsClient.TranslateDescriptorsAsync(azureDevOpsClientConfig.orgName, commaSepSDs);
                this.translatedDescriptors.AddRange(translatedDescriptors);
                this.inputDescriptors.AddRange(unknownDescriptors);
            }
        }
    }
}
