namespace UmbracoCompose.Models;

/// <summary>
/// Strongly-typed response models for GraphQL queries.
/// Used by the CLI's cross-collection composition example (query:composed command).
/// </summary>
public static class GraphQLResponseModels
{
    /// <summary>
    /// Response model for composed queries with cross-collection references.
    /// Demonstrates automatic product nesting within articles.
    /// </summary>
    public class ComposedQueryResponse
    {
        public ArticlesWithProductsConnection Articles { get; set; } = new();
    }

    /// <summary>
    /// Connection type for articles with nested products.
    /// </summary>
    public class ArticlesWithProductsConnection
    {
        public List<ArticleWithProducts> Items { get; set; } = new();
    }

    /// <summary>
    /// Article with automatically expanded product references.
    /// </summary>
    public class ArticleWithProducts
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public List<string> Categories { get; set; } = new();
        public FeaturedProductsConnection? FeaturedProducts { get; set; }
    }

    /// <summary>
    /// Connection type for featured products within articles.
    /// </summary>
    public class FeaturedProductsConnection
    {
        public List<ProductNode> Items { get; set; } = new();
    }

    /// <summary>
    /// Product node with GraphQL metadata.
    /// </summary>
    public class ProductNode
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
    }
}
