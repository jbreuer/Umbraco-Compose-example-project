using UmbracoCompose.Models;

namespace UmbracoCompose.Client.Management;

/// <summary>
/// Client for managing Umbraco Compose environments, collections, and type schemas.
/// </summary>
public interface IManagementClient
{
    /// <summary>
    /// Lists all environments in the current project.
    /// </summary>
    Task<List<EnvironmentResponse>> ListEnvironmentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new environment (e.g., dev, staging, production).
    /// </summary>
    Task<EnvironmentResponse> CreateEnvironmentAsync(string alias, string description, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an environment and all its collections and content.
    /// </summary>
    Task DeleteEnvironmentAsync(string alias, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all collections in the specified environment.
    /// </summary>
    Task<List<CollectionResponse>> ListCollectionsAsync(string environmentAlias, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets details for a specific collection.
    /// </summary>
    Task<CollectionResponse> GetCollectionAsync(string environmentAlias, string collectionAlias, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new collection in the specified environment.
    /// </summary>
    Task<CollectionResponse> CreateCollectionAsync(string environmentAlias, string collectionAlias, string description, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a collection and all its content.
    /// </summary>
    Task DeleteCollectionAsync(string environmentAlias, string collectionAlias, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all type schemas in the specified environment.
    /// </summary>
    Task<List<TypeSchemaResponse>> ListTypeSchemasAsync(string environmentAlias, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets details for a specific type schema.
    /// </summary>
    Task<TypeSchemaResponse> GetTypeSchemaAsync(string environmentAlias, string typeSchemaAlias, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new type schema from JSON Schema definition.
    /// </summary>
    Task<TypeSchemaResponse> CreateTypeSchemaAsync(string environmentAlias, string schemaJson, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a type schema from the environment.
    /// </summary>
    Task DeleteTypeSchemaAsync(string environmentAlias, string typeSchemaAlias, CancellationToken cancellationToken = default);
}
