

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stellaris.Shared.AzureDevOps;
using Stellaris.Shared.Config;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage;

namespace Stellaris.WebApi.Middleware
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext httpContext,
            [FromServices] ILogger<AuthorizationMiddleware> logger,
            [FromServices] AzureDevOpsClientConfig config, 
            [FromServices] DevOpsClient devOpsClient)
        {
            try
            {
                var connectionData = await devOpsClient.GetConnectionDataAsync(config.orgName);

                if(connectionData.AuthenticatedUser.SubjectDescriptor == null)
                {
                    throw new UnauthorizedAccessException("Access denied");
                }

                logger.LogInformation($"Connection data: {connectionData.AuthenticatedUser.ProviderDisplayName}");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogError(ex, "Error in AuthorizationMiddleware");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AuthorizationMiddleware");
                throw new UnauthorizedAccessException("Access denied", ex);
            }

            await _next(httpContext);
        }
    }

    public static class AuthorizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseDevOpsAccessTokenValidation(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthorizationMiddleware>();
        }
    }
}
