
using Microsoft.AspNetCore.Mvc;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage;
using Stellaris.WebApi.Payloads;

namespace Stellaris.WebApi.Routes
{
    public static class CustomRoleRoutes
    {
        public const string RouteName = "custom-roles";

        public static RouteGroupBuilder AddCustomRoleRoutes(this RouteGroupBuilder routeGroup)
        {
            var customRoleRoute = routeGroup.MapGroup(RouteName);

            customRoleRoute.MapGet("/", ListCustomRolesAsync);
            customRoleRoute.MapPost("/", CreateCustomRoleAsync);            
            customRoleRoute.MapPut("/{roleId}", ModifyCustomRoleAsync);
            customRoleRoute.MapDelete("/{roleId}", DeleteCustomRoleAsync);

            // routes for assignments
            customRoleRoute.MapGet("/roleAssignments/{folderId}", ListCustomRoleAssignmentsAsync);
            customRoleRoute.MapPost("/roleAssignments", CreateCustomRoleAssignmentAsync);
            customRoleRoute.MapDelete("roleAssignments/{roleAssignmentId}", DeleteCustomRoleAssignmentAsync);

            return customRoleRoute;
        }

        public static async Task<IResult> DeleteCustomRoleAssignmentAsync(
            [FromRoute] string projectId,
            [FromRoute] string roleAssignmentId,
            [FromServices] ChangeEventCosmosClient changeEventCosmosClient,
            [FromServices] CustomRoleAssignmentCosmosClient customRoleAssignmentCosmosClient)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(projectId, nameof(projectId));
            ArgumentNullException.ThrowIfNullOrEmpty(roleAssignmentId, nameof(roleAssignmentId));

            var deletedItem = await customRoleAssignmentCosmosClient.DeleteAsync(projectId, roleAssignmentId);
            if(deletedItem != null )
            {
                await changeEventCosmosClient.RaiseEventAsync(ChangeEventEntity.CreateRoleAssignmentDeletedEventFrom(deletedItem));
            }
            return Results.Ok(new { deleted = deletedItem != null });
        }

        private static async Task<CustomRoleAssignmentEntity> CreateCustomRoleAssignmentAsync(
            [FromRoute] string projectId,
            [FromBody] NewCustomRoleAssignmentPayload payload,
            [FromServices] ChangeEventCosmosClient changeEventCosmosClient,
            [FromServices] CustomRoleAssignmentCosmosClient customRoleAssignmentCosmosClient)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(projectId, nameof(projectId));
            ArgumentNullException.ThrowIfNullOrEmpty(payload.FolderId, nameof(payload.FolderId));            
            ArgumentNullException.ThrowIfNull(payload.Identity, nameof(payload.Identity));
            ArgumentNullException.ThrowIfNullOrEmpty(payload.CustomRoleId, nameof(payload.CustomRoleId));

            var roleAssignmentEntity = await customRoleAssignmentCosmosClient
                .CreateAsync(projectId, payload.FolderId, payload.Identity, payload.CustomRoleId);

            await changeEventCosmosClient.RaiseEventAsync(ChangeEventEntity.CreateRoleAssignmentCreatedEventFrom(roleAssignmentEntity));
            return roleAssignmentEntity;
        }

        private static async Task<List<CustomRoleAssignmentPayload>> ListCustomRoleAssignmentsAsync(
            [FromRoute] string projectId,
            [FromRoute] string folderId,
            [FromServices] CustomRoleCosmosClient customRoleCosmosClient,
            [FromServices] CustomRoleAssignmentCosmosClient customRoleAssignmentCosmosClient)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(projectId, nameof(projectId));
            ArgumentNullException.ThrowIfNullOrEmpty(folderId, nameof(folderId));            

            var roles = await customRoleCosmosClient.ListCustomRolesAsync(projectId);
            var roleAssignments = await customRoleAssignmentCosmosClient.ListCustomRolesAssignmentsAsync(projectId, folderId);

            var payloads = new List<CustomRoleAssignmentPayload>();
            foreach (var roleAssignment in roleAssignments)
            {
                var role = roles.Find(r => r.Id == roleAssignment.CustomRoleId);
                if (role != null && roleAssignment.Identity != null)
                {
                    var payload = new CustomRoleAssignmentPayload(roleAssignment.Id, roleAssignment.ProjectId, roleAssignment.FolderId,
                        roleAssignment.Identity, roleAssignment.CustomRoleId, role.Name);
                    payloads.Add(payload);                    
                }
            }
            return payloads;
        }

        private static async Task<List<CustomRoleDefinitionEntity>> ListCustomRolesAsync(
            [FromRoute] string projectId,
            [FromServices] CustomRoleCosmosClient customRoleCosmosClient)
        {
            var roles = await customRoleCosmosClient.ListCustomRolesAsync(projectId);
            return roles;
        }

        private static async Task<CustomRoleDefinitionEntity> CreateCustomRoleAsync(
            [FromRoute] string projectId,
            [FromBody] NewOrEditCustomRolePayload payload,
            [FromServices] ChangeEventCosmosClient changeEventCosmosClient,
            [FromServices] CustomRoleCosmosClient customRoleCosmosClient)
        {
            var customRoleEntity = await customRoleCosmosClient.CreateAsync(
                projectId, payload.Name, 
                payload.RepositoryPermissionGrants,
                payload.BuildPermissionGrants,
                payload.ReleasePermissionGrants,
                payload.ServiceEndpointRole,
                payload.EnvironmentRole,
                payload.LibraryRole);

            await changeEventCosmosClient.RaiseEventAsync(ChangeEventEntity.CreateCustomRoleCreatedEventFrom(customRoleEntity));
            return customRoleEntity;
        }

        private static async Task<CustomRoleDefinitionEntity> ModifyCustomRoleAsync(
            [FromRoute] string projectId,
            [FromRoute] string roleId,
            [FromBody] NewOrEditCustomRolePayload payload,
            [FromServices] ChangeEventCosmosClient changeEventCosmosClient,
            [FromServices] CustomRoleCosmosClient customRoleCosmosClient)
        {
            var customRoleEntity = await customRoleCosmosClient.ModifyAsync(
                projectId, roleId, payload.Name,
                payload.RepositoryPermissionGrants,
                payload.BuildPermissionGrants,
                payload.ReleasePermissionGrants,
                payload.ServiceEndpointRole,
                payload.EnvironmentRole,
                payload.LibraryRole);
            await changeEventCosmosClient.RaiseEventAsync(ChangeEventEntity.CreateCustomRoleUpdatedEventFrom(customRoleEntity));
            return customRoleEntity;
        }

        public static async Task<IResult> DeleteCustomRoleAsync(
            [FromRoute] string projectId,
            [FromRoute] string roleId,
            [FromServices] ChangeEventCosmosClient changeEventCosmosClient,
            [FromServices] CustomRoleCosmosClient customRoleCosmosClient)
        {
            var deleted = await customRoleCosmosClient.DeleteAsync(projectId, roleId);
            await changeEventCosmosClient.RaiseEventAsync(ChangeEventEntity.CreateCustomRoleDeletedEventFrom(projectId, roleId));
            return Results.Ok(new { deleted = true });
        }
    }
}
