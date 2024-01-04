

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Stellaris.Shared.AzureDevOps.Abstractions;
using Stellaris.Shared.AzureDevOps.Authorizations;
using Stellaris.Shared.AzureDevOps.DTOs;
using Stellaris.Shared.Config;
using System.Text.Json;

namespace Stellaris.Shared.AzureDevOps
{
    public class DevOpsClient : ClientBase
    {
        private readonly ILogger<DevOpsClient> logger;
        public DevOpsClient(
            IHttpContextAccessor httpContextAccessor,
            JsonSerializerOptions jsonSerializerOptions,
            AzureDevOpsClientConfig appConfiguration,
            IHttpClientFactory httpClientFactory,
            AuthorizationFactory identitySupport,
            ILogger<DevOpsClient> logger) : base(jsonSerializerOptions, httpContextAccessor,
                appConfiguration, identitySupport, httpClientFactory)
        {
            this.logger = logger;
        }

        public async Task<ConnectionDataPayload> GetConnectionDataAsync(string orgName)
        {
            var apiPath = $"_apis/ConnectionData";
            var connectionData = await this.GetAsync<ConnectionDataPayload>(orgName, apiPath, false);
            return connectionData;
        }

        public async Task<AzDoProject> GetProjectAsync(string orgName, string projectId)
        {
            var path = $"_apis/projects/{projectId}?api-version=7.2-preview.4";
            var project = await this.GetAsync<AzDoProject>(orgName, path, true);
            return project;
        }

        public async Task<List<AzDoProject>> ListProjectsAsync(string orgName, bool elevated = false)
        {
            var path = $"_apis/projects?api-version=7.2-preview.4";
            var projectCollection = await this.GetAsync<AzDoGenericCollection<AzDoProject>>(orgName, path, elevated);
            return projectCollection.Value;
        }

        public async Task<AzDoIdentity> MaterializeGroupAsync(string orgName, AzDoIdentity group)
        {
            var path = $"_apis/graph/groups?api-version=6.0-preview.1";
            var payload = new
            {
                originId = group.OriginId
            };
            var materializedResponse = await PostVsspAsync<object, AzDoGroupMaterializeResponse>(orgName, path, payload, true);

            var newGroup = new AzDoIdentity(group.EntityId, group.EntityType, group.OriginDirectory, group.OriginId, materializedResponse.Domain,
                group.LocalId, materializedResponse.PrincipalName, group.ScopeName, group.SamAccountName, materializedResponse.Descriptor, group.Department,
                group.JobTitle, group.Mail, group.MailNickname, group.PhysicalDeliveryOfficeName, group.SignInAddress, group.Surname, group.Guest,
                group.Description, group.IsMru);
            return newGroup;
        }

        public async Task<IReadOnlyList<AzDoTranslatedIdentityDescriptor>> TranslateDescriptorsAsync(string orgName, string subjectDescriptors)
        {
            var path = $"_apis/identities?api-version=7.0&subjectDescriptors={subjectDescriptors}&queryMembership=None";

            var response = await GetVsspAsync<AzDoDescriptorTranslationResponse>(orgName, path, true);
            return response.Value;
        }

        public async Task<IEnumerable<AzDoIdentity>> SearchIdentityAsync(string orgName, IdentitySearchPayload payload)
        {
            var identities = new List<AzDoIdentity>();
            var path = $"_apis/IdentityPicker/Identities?api-version=5.0-preview.1";
            var saerchResult = await PostAsync<IdentitySearchPayload, AzDoSearchResponse>(orgName, path, payload, true);
            if (saerchResult != null && saerchResult.Results != null)
            {
                foreach (var result in saerchResult.Results)
                {
                    if (result.Identities != null)
                    {
                        identities.AddRange(result.Identities);
                    }
                }
            }
            return identities;
        }

        public async Task<AzdoIdentitySlim?> GetIdentityByDescriptorAsync(string orgName, string descriptor)
        {
            var path = $"_apis/identities?api-version=7.2-preview.1&descriptors={descriptor}&queryMembership=None";

            var response = await GetVsspAsync<AzDoGenericCollection<AzdoIdentitySlim>>(orgName, path, true);
            if (response != null && response.Value != null && response.Value.Count > 0)
            {
                return response.Value[0];
            }
            return default;
        }

        public async Task<bool> CheckMembershipAsync(string orgName, string memberDescriptor, string containerDescriptor)
        {
            ArgumentNullException.ThrowIfNull(memberDescriptor);
            ArgumentNullException.ThrowIfNull(containerDescriptor);

            var path = $"_apis/graph/memberships/{memberDescriptor}/{containerDescriptor}?api-version=7.0-preview.1";
            try
            {
                // Note: this will throw an exception if the user is not a member of the group.
                var response = await GetVsspAsync<AzDoMembershipResponse>(orgName, path, true);
                if (response != null
                    && response.ContainerDescriptor.Equals(containerDescriptor, StringComparison.OrdinalIgnoreCase)
                    && response.MemberDescriptor.Equals(memberDescriptor, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Membership check failed.");
            }
            return false;
        }

        public async Task<bool> ApplyAclsAsync(string orgName, string namespaceId, AzDoAclEntryCollection[] aces)
        {
            var path = $"_apis/accesscontrollists/{namespaceId}?api-version=6.0";
            await PostAsync<AzDoAclEntryPostBody, string>(orgName, path, new AzDoAclEntryPostBody(aces), true);
            return true;
        }

        #region Environment permissions
        public async Task<bool> AddEnvironmentRolesAsync(
            string orgName, string projectId, int envId, IEnumerable<AzDoRoleAssignment> roleAssignments)
        {
            if(roleAssignments != null && roleAssignments.Any())
            {
                var path = $"_apis/securityroles/scopes/distributedtask.environmentreferencerole/roleassignments/resources/{projectId}_{envId}?api-version=5.1-preview.1";
                var result = await PutAsync<IEnumerable<AzDoRoleAssignment>, string>(orgName, path, roleAssignments, true);
                return result != null;
            }
            return false;
        }

        public async Task<bool> ChangeEnvironmentInheritanceAsync(
            string orgName, string projectId, int envId, bool inheritPermissions)
        {
            var path = $"_apis/securityroles/scopes/distributedtask.environmentreferencerole/roleassignments/resources/{projectId}_{envId}?inheritPermissions={inheritPermissions}&api-version=7.2-preview.1";
            return await PatchWithoutBodyAsync(orgName, path, true);
        }

        public async Task<bool> RemoveEnvironmentRolesAsync(
            string orgName, string projectId, int envId, IEnumerable<string> userIds)
        {
            if (userIds != null && userIds.Any())
            {
                var path = $"_apis/securityroles/scopes/distributedtask.environmentreferencerole/roleassignments/resources/{projectId}_{envId}?api-version=7.2-preview.1";
                var result = await PatchAsync<IEnumerable<string>, string>(orgName, path, userIds, true);
                return result != null;
            }
            return false;
        }

        public async Task<AzDoGenericCollection<AzDoDistributedTaskRoleAssignment>> ListEnvironmentRoleAssignmentsAsync(
            string orgName, string projectId, int envId)
        {
            var path = $"_apis/securityroles/scopes/distributedtask.environmentreferencerole/roleassignments/resources/{projectId}_{envId}?api-version=7.2-preview.1";
            return await GetAsync<AzDoGenericCollection<AzDoDistributedTaskRoleAssignment>>(orgName, path, true);
        }

        #endregion

        #region Service endpoint permissions
        public async Task<bool> AddServiceEndpointRolesAsync(
            string orgName, string projectId, string serviceEndpointId, IEnumerable<AzDoRoleAssignment> roleAssignments)
        {
            if (roleAssignments != null && roleAssignments.Any())
            {
                var path = $"_apis/securityroles/scopes/distributedtask.serviceendpointrole/roleassignments/resources/{projectId}_{serviceEndpointId}?api-version=5.0-preview.1";
                var result = await PutAsync<IEnumerable<AzDoRoleAssignment>, string>(orgName, path, roleAssignments, true);
                return result != null;
            }
            return false;
        }

        public async Task<bool> ChangeServiceEndpointInheritanceAsync(
            string orgName, string projectId, string serviceEndpointId, bool inheritPermissions)
        {
            var path = $"_apis/securityroles/scopes/distributedtask.serviceendpointrole/roleassignments/resources/{projectId}_{serviceEndpointId}?inheritPermissions={inheritPermissions}&api-version=7.2-preview.1";
            return await PatchWithoutBodyAsync(orgName, path, true);
        }

        public async Task<bool> RemoveServiceEndpointRolesAsync(
            string orgName, string projectId, string serviceEndpointId, IEnumerable<string> userIds)
        {
            if (userIds != null && userIds.Any())
            {
                var path = $"_apis/securityroles/scopes/distributedtask.serviceendpointrole/roleassignments/resources/{projectId}_{serviceEndpointId}?api-version=7.2-preview.1";
                var result = await PatchAsync<IEnumerable<string>, string>(orgName, path, userIds, true);
                return result != null;
            }
            return false;
        }

        public async Task<AzDoGenericCollection<AzDoDistributedTaskRoleAssignment>> ListServiceEndpointRoleAssignmentsAsync(
            string orgName, string projectId, string serviceEndpointId)
        {
            var path = $"_apis/securityroles/scopes/distributedtask.serviceendpointrole/roleassignments/resources/{projectId}_{serviceEndpointId}?api-version=7.2-preview.1";
            return await GetAsync<AzDoGenericCollection<AzDoDistributedTaskRoleAssignment>>(orgName, path, true);
        }

        #endregion

        #region Library permissions
        public async Task<bool> AddLibraryRolesAsync(
            string orgName, string projectId, int libraryId, IEnumerable<AzDoRoleAssignment> roleAssignments)
        {
            if (roleAssignments != null && roleAssignments.Any())
            {
                var path = $"_apis/securityroles/scopes/distributedtask.variablegroup/roleassignments/resources/{projectId}${libraryId}?api-version=7.2-preview.1";
                var result = await PutAsync<IEnumerable<AzDoRoleAssignment>, string>(orgName, path, roleAssignments, true);
                return result != null;
            }
            return false;
        }

        public async Task<bool> ChangeLibraryInheritanceAsync(
            string orgName, string projectId, int libraryId, bool inheritPermissions)
        {
            var path = $"_apis/securityroles/scopes/distributedtask.variablegroup/roleassignments/resources/{projectId}${libraryId}?inheritPermissions={inheritPermissions}&api-version=7.2-preview.1";
            return await PatchWithoutBodyAsync(orgName, path, true);
        }

        public async Task<bool> RemoveLibraryRolesAsync(
            string orgName, string projectId, int libraryId, IEnumerable<string> userIds)
        {
            if (userIds != null && userIds.Any())
            {
                var path = $"_apis/securityroles/scopes/distributedtask.variablegroup/roleassignments/resources/{projectId}${libraryId}?api-version=7.2-preview.1";
                var result = await PatchAsync<IEnumerable<string>, string>(orgName, path, userIds, true);
                return result != null;
            }
            return false;
        }

        public async Task<AzDoGenericCollection<AzDoDistributedTaskRoleAssignment>> ListLibraryRoleAssignmentsAsync(
            string orgName, string projectId, int libraryId)
        {
            var path = $"_apis/securityroles/scopes/distributedtask.variablegroup/roleassignments/resources/{projectId}${libraryId}?api-version=7.2-preview.1";
            return await GetAsync<AzDoGenericCollection<AzDoDistributedTaskRoleAssignment>>(orgName, path, true);
        }
        #endregion
    }
}
