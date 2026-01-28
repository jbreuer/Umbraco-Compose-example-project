using GraphQL;
using UmbracoCompose.Models;

namespace UmbracoCompose.Client.Query;

/// <summary>
/// Client for executing GraphQL queries against Umbraco Compose content.
/// </summary>
public interface IQueryClient
{
    /// <summary>
    /// Executes a GraphQL query against the specified environment with strongly-typed response model.
    /// Use this for type-safe queries with IntelliSense and compile-time checking.
    /// </summary>
    /// <typeparam name="T">Expected response type (e.g., ComposedQueryResponse)</typeparam>
    /// <param name="query">GraphQL query string</param>
    /// <param name="variables">Optional query variables</param>
    /// <param name="environment">Environment alias (defaults to production)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>GraphQL response with data and/or errors</returns>
    Task<GraphQLResponse<T>> ExecuteQueryAsync<T>(string query, Dictionary<string, object>? variables = null, string environment = Constants.Environments.Production, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a GraphQL query and returns JSON data.
    /// Use this for flexible querying when you want to inspect or display JSON output.
    /// </summary>
    /// <param name="query">GraphQL query string</param>
    /// <param name="variables">Optional query variables</param>
    /// <param name="environment">Environment alias (defaults to production)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JSON element containing the query data, or null if errors occurred</returns>
    Task<System.Text.Json.JsonElement?> ExecuteQueryAsJsonAsync(string query, Dictionary<string, object>? variables = null, string environment = Constants.Environments.Production, CancellationToken cancellationToken = default);
}
