using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using UmbracoCompose.Client.Authentication;
using UmbracoCompose.Models;

namespace UmbracoCompose.Client;

/// <summary>
/// Base class for Compose API clients that provides common authentication functionality.
/// </summary>
public abstract class BaseComposeClient
{
    protected readonly HttpClient HttpClient;
    protected readonly IAuthenticationService AuthService;
    protected readonly ComposeConfiguration Config;

    protected BaseComposeClient(
        HttpClient httpClient, 
        IAuthenticationService authService, 
        IOptions<ComposeConfiguration> config)
    {
        HttpClient = httpClient;
        AuthService = authService;
        Config = config.Value;
    }

    /// <summary>
    /// Gets an authenticated token (either Personal Access Token or OAuth2 token) and sets the Authorization header.
    /// Override this method to customize authentication behavior for specific clients.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The bearer token</returns>
    protected virtual async Task<string> GetAuthenticatedTokenAsync(CancellationToken cancellationToken = default)
    {
        string token;
        if (!string.IsNullOrEmpty(Config.Auth.PersonalAccessToken))
        {
            token = Config.Auth.PersonalAccessToken;
        }
        else
        {
            token = await AuthService.GetAccessTokenAsync(cancellationToken);
        }
        
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return token;
    }
}
