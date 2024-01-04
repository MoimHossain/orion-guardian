

using Microsoft.AspNetCore.Http;
using Stellaris.Shared.AzureDevOps.Authorizations;
using Stellaris.Shared.Config;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Stellaris.Shared.AzureDevOps.Abstractions
{
    public abstract class ClientBase
    {
        protected readonly JsonSerializerOptions jsonSerializerOptions;
        private readonly IHttpContextAccessor httpContextAccessor;
        protected readonly AzureDevOpsClientConfig appConfiguration;
        private readonly AuthorizationFactory identitySupport;
        protected readonly IHttpClientFactory httpClientFactory;

        public ClientBase(
            JsonSerializerOptions jsonSerializerOptions,
            IHttpContextAccessor httpContextAccessor,
            AzureDevOpsClientConfig appConfiguration,
            AuthorizationFactory identitySupport,
            IHttpClientFactory httpClientFactory)
        {
            this.jsonSerializerOptions = jsonSerializerOptions;
            this.httpContextAccessor = httpContextAccessor;
            this.appConfiguration = appConfiguration;
            this.identitySupport = identitySupport;
            this.httpClientFactory = httpClientFactory;
        }

        protected async virtual Task<TResponsePayload> PostAsync<TRequestPayload, TResponsePayload>(
            string orgName, string apiPath, TRequestPayload payload, bool elevate = false)
            where TRequestPayload : class
            where TResponsePayload : class
        {
            return await SendRequestCoreAsync<TRequestPayload, TResponsePayload>(orgName, AzureDevOpsClientConstants.CoreAPI.NAME, apiPath, payload, HttpMethod.Post, elevate);
        }

        protected async virtual Task<TResponsePayload> PutAsync<TRequestPayload, TResponsePayload>(
            string orgName, string apiPath, TRequestPayload payload, bool elevate = false)
            where TRequestPayload : class
            where TResponsePayload : class
        {
            return await SendRequestCoreAsync<TRequestPayload, TResponsePayload>(orgName, AzureDevOpsClientConstants.CoreAPI.NAME, apiPath, payload, HttpMethod.Put, elevate);
        }

        protected async virtual Task<TResponsePayload> PatchAsync<TRequestPayload, TResponsePayload>(
            string orgName, string apiPath, TRequestPayload payload, bool elevate = false)
            where TRequestPayload : class
            where TResponsePayload : class
        {
            return await SendRequestCoreAsync<TRequestPayload, TResponsePayload>(orgName, AzureDevOpsClientConstants.CoreAPI.NAME, apiPath, payload, HttpMethod.Patch, elevate);
        }

        protected async virtual Task<bool> PatchWithoutBodyAsync(
            string orgName, string apiPath, bool elevate = false)
        {
            return await SendRequestWithoutBodyCoreAsync(orgName, AzureDevOpsClientConstants.CoreAPI.NAME, apiPath, HttpMethod.Patch, elevate);
        }



        protected async virtual Task<TPayload> GetAsync<TPayload>(string orgName, string apiPath, bool elevate = false) where TPayload : class
        {
            return await GetCoreAsync<TPayload>(orgName, AzureDevOpsClientConstants.CoreAPI.NAME, apiPath, elevate);
        }

        protected async virtual Task<TPayload> GetVsspAsync<TPayload>(string orgName, string apiPath, bool elevate = false) where TPayload : class
        {
            return await GetCoreAsync<TPayload>(orgName, AzureDevOpsClientConstants.VSSPS_API.NAME, apiPath, elevate);
        }

        protected async virtual Task<TResponsePayload> PostVsspAsync<TRequestPayload, TResponsePayload>(
            string orgName, string apiPath, TRequestPayload payload, bool elevate = false)
            where TRequestPayload : class
            where TResponsePayload : class
        {
            return await SendRequestCoreAsync<TRequestPayload, TResponsePayload>(orgName, AzureDevOpsClientConstants.VSSPS_API.NAME, apiPath, payload, HttpMethod.Post, elevate);
        }

        private async Task<TPayload> GetCoreAsync<TPayload>(
            string orgName, string apiType, string apiPath, bool elevate = false) where TPayload : class
        {
            var (scheme, token) = await identitySupport.GetCredentialsAsync(elevate);
            using HttpClient client = httpClientFactory.CreateClient(apiType);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
            var path = $"/{orgName}/{apiPath}";
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var x = await response.Content.ReadAsStringAsync();
                var result = await response.Content.ReadFromJsonAsync<TPayload>(this.jsonSerializerOptions);
                if (result != null)
                {
                    return result;
                }
            }
            throw new InvalidOperationException($"Error: {response.StatusCode}");
        }

        private async Task<TResponsePayload> SendRequestCoreAsync<TRequestPayload, TResponsePayload>(
            string orgName, string apiType, string apiPath, TRequestPayload payload, HttpMethod httpMethod, bool elevate = false)
            where TRequestPayload : class
            where TResponsePayload : class
        {
            var (scheme, token) = await identitySupport.GetCredentialsAsync(elevate);
            using HttpClient client = httpClientFactory.CreateClient(apiType);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
            var path = $"/{orgName}/{apiPath}";

            using var memoryStream = new MemoryStream();
            await JsonSerializer.SerializeAsync<TRequestPayload>(memoryStream, payload, this.jsonSerializerOptions);

            var jsonContent = new StringContent(Encoding.UTF8.GetString(memoryStream.ToArray()), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(httpMethod, path)
            {
                Content = jsonContent
            };
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var x = await response.Content.ReadAsStringAsync();
                if (typeof(TResponsePayload) == typeof(string))
                {
#pragma warning disable CS8603 // Possible null reference return.
                    return (!string.IsNullOrWhiteSpace(x as string) ? x as string : string.Empty) as TResponsePayload;
#pragma warning restore CS8603 // Possible null reference return.
                }
                var result = await response.Content.ReadFromJsonAsync<TResponsePayload>(this.jsonSerializerOptions);
                if (result != null)
                {
                    return result;
                }
            }
            throw new InvalidOperationException($"Error: {response.StatusCode}");
        }

        private async Task<bool> SendRequestWithoutBodyCoreAsync(
            string orgName, string apiType, string apiPath, HttpMethod httpMethod, bool elevate = false)
        {
            var (scheme, token) = await identitySupport.GetCredentialsAsync(elevate);
            using HttpClient client = httpClientFactory.CreateClient(apiType);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
            var path = $"/{orgName}/{apiPath}";

            var request = new HttpRequestMessage(httpMethod, path);
            var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
