namespace UmbracoCompose.Client.Authentication;

/// <summary>
/// Service for managing authentication with Umbraco Compose APIs.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Retrieves an OAuth2 access token using client credentials flow.
    /// Tokens are cached and automatically refreshed when needed.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Valid access token</returns>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests authentication by requesting a token and validating it.
    /// Useful for verifying credentials during setup.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if authentication succeeded, false otherwise</returns>
    Task<bool> TestAuthenticationAsync(CancellationToken cancellationToken = default);
}
