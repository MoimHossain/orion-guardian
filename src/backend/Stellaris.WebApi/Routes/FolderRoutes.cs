

using Microsoft.AspNetCore.Mvc;
using Stellaris.Shared.Business;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage;
using Stellaris.WebApi.Payloads;

namespace Stellaris.WebApi.Routes
{
    public static class FolderRoutes
    {
        public const string RouteName = "folders";

        public static RouteGroupBuilder AddFolderRoutes(this RouteGroupBuilder routeGroup)
        {
            var folderRoute = routeGroup.MapGroup(RouteName);

            folderRoute.MapPost("/", CreateTopLevelFolderAsync);
            folderRoute.MapPost("/{parentFolderId}/children", CreateSubFolderAsync);
            folderRoute.MapPut("/{id}", UpdateFolderAsync);
            folderRoute.MapDelete("/{id}", DeleteFolderAsync);
            folderRoute.MapGet("/", ListFoldersAsync);            
            folderRoute.MapGet("/{parentFolderId}/children", ListChildFoldersAsync);            
            return folderRoute;
        }



        private static async Task<IResult> DeleteFolderAsync(
            [FromRoute] string projectId,
            [FromRoute] string id,
            [FromServices] FolderCosmosClient folderCosmosClient,
            [FromServices] LinkCosmosClient linkCosmosClient,
            [FromServices] SecurityCosmosClient securityCosmosClient)
        {
            var deleted = await folderCosmosClient.DeleteFolderAsync(projectId, id);

            var links = await linkCosmosClient.ListLinksByFolderIdAsync(projectId, id);
            if (links != null)
            {
                foreach (var link in links)
                {
                    await linkCosmosClient.DeleteLinkAsync(projectId, link.Id);
                }
            }
            var securityDescriptors = await securityCosmosClient.ListSecurityDescriptorsByFolderIdAsync(projectId, id);
            if (securityDescriptors != null)
            {
                foreach (var sd in securityDescriptors)
                {
                    await securityCosmosClient.DeleteSecurityDescriptorAsync(projectId, sd.Id);
                }
            }

            return Results.Ok(new { deleted = true });
        }

        private static async Task<List<FolderWithPermissionEntity>> ListFoldersAsync(
            [FromRoute] string projectId,
            [FromServices] FolderCosmosClient folderCosmosClient,
            [FromServices] FolderPermissionsEvaluationService folderPermissionsEvaluationService)
        {
            var folders = await folderCosmosClient.ListRootFoldersAsync(projectId);
            return await folderPermissionsEvaluationService.PerformSecurityTrimmingAsync(projectId, folders);
        }



        private static async Task<List<FolderWithPermissionEntity>> ListChildFoldersAsync(
            [FromRoute] string projectId,
            [FromRoute] string parentFolderId,
            [FromServices] FolderCosmosClient folderCosmosClient,
            [FromServices] FolderPermissionsEvaluationService folderPermissionsEvaluationService)
        {
            var folders = await folderCosmosClient.ListChildFoldersAsync(projectId, parentFolderId);
            return await folderPermissionsEvaluationService.PerformSecurityTrimmingAsync(projectId, folders);
        }

        private static async Task<FolderEntity> UpdateFolderAsync(
            [FromRoute] string projectId,
            [FromRoute] string id,
            [FromBody] ModifyFolderPayload payload,
            [FromServices] FolderCosmosClient folderCosmosClient)
        {
            var folder = await folderCosmosClient.UpdateFolderAsync(
                               projectId, id, payload.Name, payload.Kind,
                               payload.Description, payload.CINumber, payload.LastModifiedBy);
            return folder;
        }

        private static async Task<FolderEntity> CreateTopLevelFolderAsync(
            [FromRoute] string projectId,
            [FromBody] NewFolderPayload payload,
            [FromServices] FolderCosmosClient folderCosmosClient)
        {
            var folderEntity = await folderCosmosClient
                .CreateTopLevelFolderAsync(
                projectId, payload.Name, payload.Kind, 
                payload.Description, payload.CINumber, 
                payload.CreatedBy);

            return folderEntity;
        }

        private static async Task<FolderEntity> CreateSubFolderAsync(
            [FromRoute] string projectId,
            [FromRoute] string parentFolderId,
            NewFolderPayload payload,
            [FromServices] FolderCosmosClient folderCosmosClient)
        {
            var folderEntity = await folderCosmosClient
                .CreateSubFolderAsync(projectId, parentFolderId, payload.Name, payload.Kind, 
                payload.Description, payload.CINumber,
                payload.CreatedBy);

            return folderEntity;
        }
    }
}