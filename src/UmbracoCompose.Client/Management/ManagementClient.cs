using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Options;
using UmbracoCompose.Client.Authentication;
using UmbracoCompose.Models;

namespace UmbracoCompose.Client.Management;

public class ManagementClient : BaseComposeClient, IManagementClient
{
    public ManagementClient(HttpClient httpClient, IAuthenticationService authService, IOptions<ComposeConfiguration> config)
        : base(httpClient, authService, config)
    {
        HttpClient.BaseAddress = new Uri(Config.Endpoints.Management);
    }

    protected override async Task<string> GetAuthenticatedTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = await AuthService.GetAccessTokenAsync(cancellationToken);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return token;
    }

    public async Task<List<EnvironmentResponse>> ListEnvironmentsAsync(CancellationToken cancellationToken = default)
    {
        await GetAuthenticatedTokenAsync(cancellationToken);
        var response = await HttpClient.GetAsync($"/v1/projects/{Config.Project.Alias}/environments", cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var listResponse = await response.Content.ReadFromJsonAsync<EnvironmentsListResponse>(cancellationToken);
        return listResponse?.Edges.Select(e => e.Node).ToList() ?? new();
    }

    public async Task<EnvironmentResponse> CreateEnvironmentAsync(string alias, string description, CancellationToken cancellationToken = default)
    {
        await GetAuthenticatedTokenAsync(cancellationToken);
        var payload = new { environmentAlias = alias, description };
        var response = await HttpClient.PostAsJsonAsync($"/v1/projects/{Config.Project.Alias}/environments", payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EnvironmentResponse>(cancellationToken) 
            ?? throw new InvalidOperationException("Failed to create environment");
    }

    public async Task DeleteEnvironmentAsync(string alias, CancellationToken cancellationToken = default)
    {
        await GetAuthenticatedTokenAsync(cancellationToken);
        var response = await HttpClient.DeleteAsync($"/v1/projects/{Config.Project.Alias}/environments/{alias}", cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Failed to delete environment: {response.StatusCode}\nResponse: {errorContent}");
        }
    }

    public async Task<List<CollectionResponse>> ListCollectionsAsync(string environmentAlias, CancellationToken cancellationToken = default)
    {
        await GetAuthenticatedTokenAsync(cancellationToken);
        var response = await HttpClient.GetAsync($"/v1/projects/{Config.Project.Alias}/environments/{environmentAlias}/collections", cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var listResponse = await response.Content.ReadFromJsonAsync<CollectionsListResponse>(cancellationToken);
        return listResponse?.Edges.Select(e => e.Node).ToList() ?? new();
    }

    public async Task<CollectionResponse> GetCollectionAsync(string environmentAlias, string collectionAlias, CancellationToken cancellationToken = default)
    {
        await GetAuthenticatedTokenAsync(cancellationToken);
        var response = await HttpClient.GetAsync($"/v1/projects/{Config.Project.Alias}/environments/{environmentAlias}/collections/{collectionAlias}", cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Failed to get collection: {response.StatusCode}\nResponse: {errorContent}");
        }
        
        return await response.Content.ReadFromJsonAsync<CollectionResponse>(cancellationToken) 
            ?? throw new InvalidOperationException("Failed to parse collection response");
    }

    public async Task<CollectionResponse> CreateCollectionAsync(string environmentAlias, string collectionAlias, string description, CancellationToken cancellationToken = default)
    {
        await GetAuthenticatedTokenAsync(cancellationToken);
        var payload = new { collectionAlias, description };
        var response = await HttpClient.PostAsJsonAsync($"/v1/projects/{Config.Project.Alias}/environments/{environmentAlias}/collections", payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CollectionResponse>(cancellationToken) 
            ?? throw new InvalidOperationException("Failed to create collection");
    }

    public async Task DeleteCollectionAsync(string environmentAlias, string collectionAlias, CancellationToken cancellationToken = default)
    {
        await GetAuthenticatedTokenAsync(cancellationToken);
        var response = await HttpClient.DeleteAsync($"/v1/projects/{Config.Project.Alias}/environments/{environmentAlias}/collections/{collectionAlias}", cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Failed to delete collection: {response.StatusCode}\nResponse: {errorContent}");
        }
    }

    public async Task<List<TypeSchemaResponse>> ListTypeSchemasAsync(string environmentAlias, CancellationToken cancellationToken = default)
    {
        await GetAuthenticatedTokenAsync(cancellationToken);
        var response = await HttpClient.GetAsync($"/v1/projects/{Config.Project.Alias}/environments/{environmentAlias}/type-schemas", cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var listResponse = await response.Content.ReadFromJsonAsync<TypeSchemasListResponse>(cancellationToken);
        return listResponse?.Edges.Select(e => e.Node).ToList() ?? new();
    }

    public async Task<TypeSchemaResponse> GetTypeSchemaAsync(string environmentAlias, string typeSchemaAlias, CancellationToken cancellationToken = default)
    {
        await GetAuthenticatedTokenAsync(cancellationToken);
        var response = await HttpClient.GetAsync($"/v1/projects/{Config.Project.Alias}/environments/{environmentAlias}/type-schemas/{typeSchemaAlias}", cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Failed to get type schema: {response.StatusCode}\nResponse: {errorContent}");
        }
        
        return await response.Content.ReadFromJsonAsync<TypeSchemaResponse>(cancellationToken) 
            ?? throw new InvalidOperationException("Failed to parse type schema response");
    }

    public async Task<TypeSchemaResponse> CreateTypeSchemaAsync(string environmentAlias, string schemaJson, CancellationToken cancellationToken = default)
    {
        await GetAuthenticatedTokenAsync(cancellationToken);
        var content = new StringContent(schemaJson, Encoding.UTF8, "application/json");
        var response = await HttpClient.PostAsync($"/v1/projects/{Config.Project.Alias}/environments/{environmentAlias}/type-schemas", content, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Failed to create type schema: {response.StatusCode}\nResponse: {errorContent}");
        }
        
        return await response.Content.ReadFromJsonAsync<TypeSchemaResponse>(cancellationToken) 
            ?? throw new InvalidOperationException("Failed to parse type schema response");
    }

    public async Task DeleteTypeSchemaAsync(string environmentAlias, string typeSchemaAlias, CancellationToken cancellationToken = default)
    {
        await GetAuthenticatedTokenAsync(cancellationToken);
        var response = await HttpClient.DeleteAsync($"/v1/projects/{Config.Project.Alias}/environments/{environmentAlias}/type-schemas/{typeSchemaAlias}", cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Failed to delete type schema: {response.StatusCode}\nResponse: {errorContent}");
        }
    }
}
