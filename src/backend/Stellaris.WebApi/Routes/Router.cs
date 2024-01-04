

namespace Stellaris.WebApi.Routes
{
    public static class Router
    {
        public const string PROJECT_ROUTE = "{projectId}";
        public const string DEVOPS_ROUTE = "devops/{orgName}";

        public static WebApplication MapApiRoutes(this WebApplication app)
        {
            var apiGroup = app.MapGroup("api");
            var projectApiRoute = apiGroup.MapGroup(PROJECT_ROUTE).WithOpenApi();
            projectApiRoute.AddFolderRoutes();
            projectApiRoute.AddLinkRoutes();
            projectApiRoute.AddSecurityRoutes();
            projectApiRoute.AddCustomRoleRoutes();

            var devOpsApiRoute = apiGroup.MapGroup(DEVOPS_ROUTE).WithOpenApi();
            devOpsApiRoute.AddDevOpsRoutes();
            return app;
        }
    }
}
