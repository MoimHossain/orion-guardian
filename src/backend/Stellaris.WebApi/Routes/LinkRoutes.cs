

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage;
using Stellaris.WebApi.Payloads;
using System.Net;

namespace Stellaris.WebApi.Routes
{
    public static class LinkRoutes
    {
        public const string RouteName = "links";

        public static RouteGroupBuilder AddLinkRoutes(this RouteGroupBuilder routeGroup)
        {
            var linkRoute = routeGroup.MapGroup(RouteName).WithOpenApi();
            linkRoute.MapPost("/", CreateLinkAsync);
            linkRoute.MapDelete("/{linkId}", DeleteLinkAsync);
            linkRoute.MapGet("/{folderId}/{kind}", ListLinksAsync);
            return linkRoute;
        }

        public static async Task<IResult> DeleteLinkAsync(
            [FromRoute] string projectId,
            [FromRoute] string linkId,
            [FromServices] LinkCosmosClient linkCosmosClient)
        {
            var deleted = await linkCosmosClient.DeleteLinkAsync(projectId, linkId);
            return Results.Ok(new { deleted = true });
        }

        public static async Task<LinkEntity?> CreateLinkAsync(
            [FromRoute] string projectId,
            [FromBody] NewLinkPayload payload,
            [FromServices] ChangeEventCosmosClient changeEventCosmosClient,
            [FromServices] LinkCosmosClient linkCosmosClient)
        {
            var link = await linkCosmosClient.CreateLinkAsync(
                projectId, payload.FolderId, payload.ResourceId, payload.Kind, payload.ResourceName, payload.By);

            await changeEventCosmosClient.RaiseEventAsync(ChangeEventEntity.CreateLinkCreatedEventFrom(link));
            return link;
        }

        public static async Task<List<LinkEntity>> ListLinksAsync(
            [FromRoute] string projectId,
            [FromRoute] string folderId,
            [FromRoute] string kind,
            [FromServices] LinkCosmosClient linkCosmosClient)
        {
            var links = await linkCosmosClient.ListLinksByFolderIdAsync(projectId, kind, folderId);
            return links;
        }
    }
}