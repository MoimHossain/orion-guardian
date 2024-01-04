
using Microsoft.AspNetCore.Mvc;
using Stellaris.Shared.Business;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage;
using Stellaris.WebApi.Payloads;

namespace Stellaris.WebApi.Routes
{
    public static class SecurityRoutes
    {
        public const string RouteName = "security";

        public static RouteGroupBuilder AddSecurityRoutes(this RouteGroupBuilder routeGroup)
        {
            var securityRoute = routeGroup.MapGroup(RouteName).WithOpenApi();
            securityRoute.MapPost("/permissions", CreateOrUpdatePermissionDescriptorAsync);
            securityRoute.MapPost("/roles", CreateOrUpdateRoleDescriptorAsync);
            securityRoute.MapGet("/currentUserRole", GetCurrentUserRoleAsync);

            securityRoute.MapGet("/{kind}", ListSecurityDescriptorsAsync);
            securityRoute.MapGet("/{folderId}/{kind}", ListSecurityDescriptorsByFolderIdAsync);
            securityRoute.MapGet("/{folderId}/evaluatePermissions", GetEvaluatedPermissionsAsync);

            return securityRoute;
        }

        private static async Task<BuiltInUserPrivilege> GetCurrentUserRoleAsync(
            [FromRoute] string projectId,
            [FromServices] BuiltInAccountService builtInAccountService)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(projectId, nameof(projectId));

            return await builtInAccountService.GetCurrentUserBuiltInRolesAsync(projectId);
        }

        private static async Task<FolderSecurityEvaluationResult> GetEvaluatedPermissionsAsync(
            [FromRoute] string projectId,
            [FromRoute] string folderId,
            [FromServices] FolderPermissionsEvaluationService folderPermissionsEvaluationService)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(projectId, nameof(projectId));
            ArgumentException.ThrowIfNullOrWhiteSpace(folderId, nameof(folderId));

            var result = await folderPermissionsEvaluationService.GetEvaluatedFolderPermissionsAsync(projectId, folderId);
            return result;
        }

        public static async Task<SecurityDescriptorEntity> CreateOrUpdateRoleDescriptorAsync(
            [FromRoute] string projectId,
            [FromBody] RoleDescriptorPayload payload,
            [FromServices] SecurityCosmosClient securityCosmosClient)
        {
            var descriptorEntity = await securityCosmosClient.CreateOrUpdateRoleDescriptorAsync(
                                              projectId, payload.FolderId, payload.Kind, payload.RoleDescriptors);
            return descriptorEntity;
        }

        public static async Task<SecurityDescriptorEntity> CreateOrUpdatePermissionDescriptorAsync(
            [FromRoute] string projectId,
            [FromBody] PermissionDescriptorPayload payload,
            [FromServices] SecurityCosmosClient securityCosmosClient)
        {
            var descriptorEntity = await securityCosmosClient.CreateOrUpdatePermissionDescriptorAsync(
                               projectId, payload.FolderId, payload.Kind, payload.PermissionDescriptors);
            return descriptorEntity;
        }

        public static async Task<List<SecurityDescriptorEntity>> ListSecurityDescriptorsAsync(
            [FromRoute] string projectId,
            [FromRoute] string kind,
            [FromServices] SecurityCosmosClient securityCosmosClient)
        {
            var descriptorEntities = await securityCosmosClient.ListSecurityDescriptorsAsync(projectId, kind);
            return descriptorEntities;
        }

        public static async Task<List<SecurityDescriptorEntity>> ListSecurityDescriptorsByFolderIdAsync(
            [FromRoute] string projectId,
            [FromRoute] string folderId,
            [FromRoute] string kind,
            [FromServices] SecurityCosmosClient securityCosmosClient)
        {
            var descriptorEntities = await securityCosmosClient.ListSecurityDescriptorsAsync(projectId, folderId, kind);
            return descriptorEntities;
        }
    }
}