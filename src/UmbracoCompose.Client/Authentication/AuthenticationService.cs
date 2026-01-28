using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using UmbracoCompose.Models;

namespace UmbracoCompose.Client.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly ComposeConfiguration _config;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public AuthenticationService(HttpClient httpClient, IOptions<ComposeConfiguration> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        // Return cached token if still valid (with 5 minute buffer)
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
        {
            return _cachedToken;
        }

        // Use the correct authentication endpoint from documentation
        // https://umbraco.gitbook.io/umbraco-orchestration/getting-started/access-control
        var endpoint = $"{_config.Endpoints.Management}/v1/auth/token";

        try
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _config.Auth.ClientId),
                new KeyValuePair<string, string>("client_secret", _config.Auth.ClientSecret)
            });

            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponseDto>(cancellationToken);
                if (tokenResponse?.AccessToken != null)
                {
                    _cachedToken = tokenResponse.AccessToken;
                    _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                    Console.WriteLine($"✓ Authentication successful!");
                    Console.WriteLine($"Token expires in {tokenResponse.ExpiresIn} seconds");
                    return _cachedToken;
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException($"Authentication failed: {response.StatusCode} - {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to authenticate: {ex.Message}");
            throw;
        }

        throw new InvalidOperationException("Unable to authenticate. Please verify your credentials.");
    }

    public async Task<bool> TestAuthenticationAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Testing authentication...");
        Console.WriteLine($"Client ID: {_config.Auth.ClientId}\n");

        try
        {
            var token = await GetAccessTokenAsync(cancellationToken);
            Console.WriteLine($"✓ Token received: {token[..50]}...");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Authentication failed: {ex.Message}");
            return false;
        }
    }

    private class TokenResponseDto
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }
}
