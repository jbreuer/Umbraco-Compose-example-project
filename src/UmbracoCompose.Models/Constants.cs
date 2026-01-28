namespace UmbracoCompose.Models;

/// <summary>
/// Common constants used throughout the Umbraco Compose client library.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Content ingestion actions.
    /// </summary>
    public static class ContentActions
    {
        /// <summary>
        /// Upsert action - creates new content or updates existing content.
        /// </summary>
        public const string Upsert = "upsert";

        /// <summary>
        /// Delete action - removes content from the collection.
        /// </summary>
        public const string Delete = "delete";
    }

    /// <summary>
    /// Content status values.
    /// </summary>
    public static class ContentStatus
    {
        /// <summary>
        /// Published status - content is live and visible.
        /// </summary>
        public const string Published = "published";

        /// <summary>
        /// Draft status - content is not yet published.
        /// </summary>
        public const string Draft = "draft";
    }

    /// <summary>
    /// Content type names.
    /// </summary>
    public static class ContentTypes
    {
        /// <summary>
        /// Article content type.
        /// </summary>
        public const string Article = "article";

        /// <summary>
        /// Product content type.
        /// </summary>
        public const string Product = "product";
    }

    /// <summary>
    /// Collection aliases.
    /// </summary>
    public static class Collections
    {
        /// <summary>
        /// Articles collection.
        /// </summary>
        public const string Articles = "articles";

        /// <summary>
        /// Products collection.
        /// </summary>
        public const string Products = "products";
    }

    /// <summary>
    /// Environment aliases.
    /// </summary>
    public static class Environments
    {
        /// <summary>
        /// Production environment.
        /// </summary>
        public const string Production = "production";

        /// <summary>
        /// Development environment.
        /// </summary>
        public const string Development = "dev";

        /// <summary>
        /// Staging environment.
        /// </summary>
        public const string Staging = "staging";
    }
}
