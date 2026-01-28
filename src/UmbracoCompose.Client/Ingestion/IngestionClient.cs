using Microsoft.Extensions.Options;
using UmbracoCompose.Client.Authentication;
using UmbracoCompose.Models;

namespace UmbracoCompose.Client.Ingestion;

public class IngestionClient : BaseComposeClient, IIngestionClient
{
    public IngestionClient(HttpClient httpClient, IAuthenticationService authService, IOptions<ComposeConfiguration> config)
        : base(httpClient, authService, config)
    {
        HttpClient.BaseAddress = new Uri(Config.Endpoints.Ingestion);
    }

    public async Task<string> IngestJsonAsync(string environmentAlias, string collectionAlias, string jsonPayload, CancellationToken cancellationToken = default)
    {
        await GetAuthenticatedTokenAsync(cancellationToken);
        
        // Ingestion API endpoint: /v1/{projectAlias}/{environmentAlias}/{collectionAlias}
        var url = $"/v1/{Config.Project.Alias}/{environmentAlias}/{collectionAlias}";

        var content = new System.Net.Http.StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
        var response = await HttpClient.PutAsync(url, content, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Ingestion failed: {response.StatusCode} - {error}");
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
