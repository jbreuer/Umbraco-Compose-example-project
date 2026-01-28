using System.Text.Json;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Options;
using UmbracoCompose.Client.Authentication;
using UmbracoCompose.Models;

namespace UmbracoCompose.Client.Query;

public class QueryClient : BaseComposeClient, IQueryClient
{
    private GraphQLHttpClient? _graphQLClient;

    public QueryClient(HttpClient httpClient, IAuthenticationService authService, IOptions<ComposeConfiguration> config)
        : base(httpClient, authService, config)
    {
        // GraphQLHttpClient will be initialized in GetOrCreateGraphQLClient
        HttpClient.BaseAddress = new Uri(Config.Endpoints.GraphQL);
    }

    private async Task<GraphQLHttpClient> GetOrCreateGraphQLClientAsync(string environment, CancellationToken cancellationToken)
    {
        // Set authentication token
        await GetAuthenticatedTokenAsync(cancellationToken);

        // Create GraphQL client if not already created or if endpoint changed
        var endpoint = $"{Config.Endpoints.GraphQL}/{Config.Project.Alias}/{environment}";
        
        if (_graphQLClient == null || (_graphQLClient.Options.EndPoint?.ToString() ?? "") != endpoint)
        {
            _graphQLClient?.Dispose();
            _graphQLClient = new GraphQLHttpClient(
                new GraphQLHttpClientOptions
                {
                    EndPoint = new Uri(endpoint)
                },
                new SystemTextJsonSerializer(),
                HttpClient);
        }

        return _graphQLClient;
    }

    public async Task<GraphQLResponse<T>> ExecuteQueryAsync<T>(
        string query, 
        Dictionary<string, object>? variables = null, 
        string environment = Constants.Environments.Production, 
        CancellationToken cancellationToken = default)
    {
        var client = await GetOrCreateGraphQLClientAsync(environment, cancellationToken);

        var request = new GraphQLRequest
        {
            Query = query,
            Variables = variables
        };

        return await client.SendQueryAsync<T>(request, cancellationToken);
    }

    public async Task<JsonElement?> ExecuteQueryAsJsonAsync(
        string query, 
        Dictionary<string, object>? variables = null, 
        string environment = Constants.Environments.Production, 
        CancellationToken cancellationToken = default)
    {
        var client = await GetOrCreateGraphQLClientAsync(environment, cancellationToken);

        var request = new GraphQLRequest
        {
            Query = query,
            Variables = variables
        };

        // Query as JsonDocument to get raw JSON
        var response = await client.SendQueryAsync<JsonDocument>(request, cancellationToken);

        // Check for errors
        if (response.Errors?.Any() == true)
        {
            var errorMessages = string.Join(", ", response.Errors.Select(e => e.Message));
            throw new InvalidOperationException($"GraphQL query returned errors: {errorMessages}");
        }

        // Return the data as JsonElement
        return response.Data?.RootElement.Clone();
    }
}
