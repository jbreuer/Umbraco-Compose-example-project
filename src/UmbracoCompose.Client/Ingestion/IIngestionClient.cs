namespace UmbracoCompose.Client.Ingestion;

/// <summary>
/// Client for ingesting content into Umbraco Compose collections.
/// </summary>
public interface IIngestionClient
{
    /// <summary>
    /// Ingests content using raw JSON payload.
    /// Payload should be an array of objects with id, type, data, and action fields.
    /// </summary>
    /// <param name="environmentAlias">Environment alias (e.g., "production", "dev")</param>
    /// <param name="collectionAlias">Collection alias (e.g., "articles", "products")</param>
    /// <param name="jsonPayload">JSON array of content items</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response from the ingestion API</returns>
    Task<string> IngestJsonAsync(string environmentAlias, string collectionAlias, string jsonPayload, CancellationToken cancellationToken = default);
}
