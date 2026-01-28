namespace UmbracoCompose.Models;

public class ComposeConfiguration
{
    public ProjectConfig Project { get; set; } = new();
    public AuthConfig Auth { get; set; } = new();
    public EndpointsConfig Endpoints { get; set; } = new();
}

public class ProjectConfig
{
    public string Alias { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class AuthConfig
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string PersonalAccessToken { get; set; } = string.Empty;
}

public class EndpointsConfig
{
    public string Management { get; set; } = string.Empty;
    public string Ingestion { get; set; } = string.Empty;
    public string GraphQL { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}
