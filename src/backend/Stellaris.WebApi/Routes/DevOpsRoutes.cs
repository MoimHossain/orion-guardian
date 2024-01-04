

using Microsoft.AspNetCore.Mvc;
using Stellaris.Shared.AzureDevOps;
using Stellaris.Shared.AzureDevOps.DTOs;

namespace Stellaris.WebApi.Routes
{
    public static class DevOpsRoutes
    {
        public const string ProjectRouteName = "projects";
        public const string IdentityRouteName = "identity";
        public static RouteGroupBuilder AddDevOpsRoutes(this RouteGroupBuilder routeGroup)
        {
            AddProjectRoutes(routeGroup);
            AddIdentityRoutes(routeGroup);

            return routeGroup;
        }

        private static RouteGroupBuilder AddIdentityRoutes(RouteGroupBuilder routeGroup)
        {
            var identityRoutes = routeGroup.MapGroup(IdentityRouteName);
            identityRoutes.MapPost("/search", SearchIdentityAsync);
            identityRoutes.MapPost("/materialize", MaterializeGroupAsync);
            return identityRoutes;
        }

        private static RouteGroupBuilder AddProjectRoutes(RouteGroupBuilder routeGroup)
        {
            var projectRoutes = routeGroup.MapGroup(ProjectRouteName);
            projectRoutes.MapGet("/", ListProjectsAsync);
            return projectRoutes;
        }

        private static async Task<List<AzDoProject>> ListProjectsAsync(
            [FromRoute] string orgName,
            [FromServices] DevOpsClient devOpsClient)
        {
            return await devOpsClient.ListProjectsAsync(orgName, elevated: true);
        }

        private static async Task<IEnumerable<AzDoIdentity>> SearchIdentityAsync(
            [FromRoute] string orgName,
            [FromBody] IdentitySearchPayload payload,
            [FromServices] DevOpsClient devOpsClient)
        {
            return await devOpsClient.SearchIdentityAsync(orgName, payload);
        }

        public static async Task<AzDoIdentity> MaterializeGroupAsync(
            [FromRoute] string orgName,
            [FromBody] AzDoIdentity group,
            [FromServices] DevOpsClient devOpsClient)
        {
            return await devOpsClient.MaterializeGroupAsync(orgName, group);
        }
    }
}
