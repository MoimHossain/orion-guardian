

using Microsoft.AspNetCore.Mvc;
using Stellaris.Shared.Schemas;
using Stellaris.Shared.Storage;

namespace Stellaris.WebApi.Middleware
{
    public class AuditMiddleware
    {
        private const string GENERAL_AUDIT_PARTITION = "general";

        private readonly RequestDelegate _next;

        public AuditMiddleware(RequestDelegate next)
        {
            _next = next;
        }


        public async Task InvokeAsync(HttpContext httpContext, [FromServices] AuditCosmosClient auditCosmosClient)
        {
            if ((httpContext.Request.Method == "POST" || httpContext.Request.Method == "PATCH"))
            {
                httpContext.Request.EnableBuffering();
            }

            await _next(httpContext);

            if (!httpContext.Request.RouteValues.TryGetValue("projectId", out var projectId)
                 || string.IsNullOrWhiteSpace($"{projectId}"))
            {
                projectId = GENERAL_AUDIT_PARTITION;
            }

            await auditCosmosClient
                .CreateAuditEventAsync(
                    projectId: $"{projectId}",
                    auditEntity: await PopulateAuditEvent($"{projectId}", httpContext));
        }

        private async static Task<AuditEntity> PopulateAuditEvent(string projectId, HttpContext httpContext)
        {
            // later fix the following with property JWT exploration and remove auth headers
            return new AuditEntity
            {
                Id = Guid.NewGuid().ToString(),
                ProjectId = projectId,
                CreatedOn = DateTime.UtcNow,
                Properties = new Dictionary<string, object>
                {
                    { "request", new
                        {
                            httpContext.Request.Method,
                            httpContext.Request.Path,
                            httpContext.Request.QueryString,
                            httpContext.Request.Headers,
                            body = await GetHttpRequestBody(httpContext)
                        }
                    },
                    { "response", new
                        {
                            httpContext.Response.StatusCode,
                            httpContext.Response.Headers
                        }
                    }
                }
            };
        }

        private async static Task<string> GetHttpRequestBody(HttpContext httpContext)
        {
            if (!(httpContext.Request.Method == "POST" || httpContext.Request.Method == "PATCH"))
                return string.Empty;

            httpContext.Request.Body.Position = 0;
            var bodyAsText = await new System.IO.StreamReader(httpContext.Request.Body).ReadToEndAsync();

            return bodyAsText;
        }
    }

    public static class AuditMiddlewareExtensions
    {
        public static IApplicationBuilder UseAudits(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuditMiddleware>();
        }
    }
}
