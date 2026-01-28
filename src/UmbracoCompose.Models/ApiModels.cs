namespace UmbracoCompose.Models;

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string Scope { get; set; } = string.Empty;
}

public class EnvironmentResponse
{
    public string EnvironmentAlias { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class EnvironmentEdge
{
    public string Cursor { get; set; } = string.Empty;
    public EnvironmentResponse Node { get; set; } = new();
}

public class EnvironmentsListResponse
{
    public List<EnvironmentEdge> Edges { get; set; } = new();
    public object? PageInfo { get; set; }
    public int? TotalCount { get; set; }
}

public class CollectionResponse
{
    public string CollectionAlias { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CollectionEdge
{
    public string Cursor { get; set; } = string.Empty;
    public CollectionResponse Node { get; set; } = new();
}

public class CollectionsListResponse
{
    public List<CollectionEdge> Edges { get; set; } = new();
    public object? PageInfo { get; set; }
    public int? TotalCount { get; set; }
}

public class TypeSchemaResponse
{
    public string TypeSchemaAlias { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<FieldDefinition> Fields { get; set; } = new();
}

public class TypeSchemaEdge
{
    public string Cursor { get; set; } = string.Empty;
    public TypeSchemaResponse Node { get; set; } = new();
}

public class TypeSchemasListResponse
{
    public List<TypeSchemaEdge> Edges { get; set; } = new();
    public object? PageInfo { get; set; }
    public int? TotalCount { get; set; }
}

public class FieldDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Required { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class ApiApplicationDto
{
    public string ApiApplicationAlias { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string? ClientSecret { get; set; }
    public ApiApplicationScopes? Scopes { get; set; }
}

public class ApiApplicationScopes
{
    public string[] Project { get; set; } = Array.Empty<string>();
    public Dictionary<string, string[]> Environments { get; set; } = new();
}

public class ApiApplicationEdge
{
    public string Cursor { get; set; } = string.Empty;
    public ApiApplicationDto Node { get; set; } = new();
}

public class GetProjectApiApplicationsResponse
{
    public List<ApiApplicationEdge> Edges { get; set; } = new();
    public object? PageInfo { get; set; }
    public int? TotalCount { get; set; }
}
