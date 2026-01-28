# Umbraco Compose - example project

An example C# client library and CLI for **[Umbraco Compose](https://umbraco.com/products/umbraco-compose/)**. Demonstrates cross-collection content composition with automatic reference expansion.

> **Note:** This example focuses on Compose's core APIs using articles and products. It does not include Umbraco CMS integration.

## Prerequisites

- .NET 10.0 SDK or later
- An Umbraco Compose account
- Personal Access Token and OAuth Client Credentials from [Umbraco Cloud Portal](https://www.s1.umbraco.io/compose)

## Quick Start

### 1. Clone and Configure

```bash
git clone https://github.com/jbreuer/Umbraco-Compose-example-project.git
cd Umbraco-Compose-example-project
```

Copy `src/UmbracoCompose.CLI/appsettings.example.json` to `src/UmbracoCompose.CLI/appsettings.json` and edit with your credentials (project alias, region, client ID, client secret, personal access token).

### 2. Run Commands

From the `src/UmbracoCompose.CLI` folder:

```bash
# Test authentication
dotnet run -- auth:test

# Create environment and collections
dotnet run -- setup:dev
dotnet run -- collections:create dev articles "Articles Collection"
dotnet run -- collections:create dev products "Products Collection"

# Create schemas
dotnet run -- schemas:create dev article
dotnet run -- schemas:create dev product

# Ingest content (products first, then articles)
dotnet run -- ingest:products-bulk dev
dotnet run -- ingest:articles-bulk dev

# Query content
dotnet run -- query:articles dev
dotnet run -- query:products dev

# 🎯 Query with automatic product nesting (demonstrates Compose's core value!)
dotnet run -- query:composed dev
```

**The `query:composed` command** shows products automatically nested inside articles, demonstrating how Compose unifies content from multiple sources with zero client-side matching code.

## Common Commands

```bash
# Environment management
dotnet run -- setup:list
dotnet run -- setup:delete [[env]]

# Content operations
dotnet run -- ingest:articles-bulk [[env]]
dotnet run -- ingest:products-bulk [[env]]
dotnet run -- delete:items-bulk [[env]]

# Queries
dotnet run -- query:articles [[env]]
dotnet run -- query:products [[env]]
dotnet run -- query:composed [[env]]              # Cross-collection with auto-expansion
dotnet run -- query:typed [[env]]                 # Strongly-typed example
dotnet run -- query:articles-filtered [[env]]
dotnet run -- query:articles-sorted [[env]]
dotnet run -- query:articles-paginated [[env]] [[size]]

# Schema management
dotnet run -- schemas:list [[env]]
dotnet run -- schemas:create [[env]] [[type]]
dotnet run -- schemas:delete [[env]] [[alias]]
```

Run `dotnet run` without arguments to see all available commands.

## Key Feature: Cross-Collection Composition

This project demonstrates Compose's ability to automatically expand referenced content across collections:

**Schema configuration** (`article-schema-compose.json`):
```json
"featuredProducts": {
  "type": "array",
  "items": {
    "type": "object",
    "$ref": "product"
  },
  "$delivery": {
    "refCollection": "products"
  }
}
```

**Ingestion** (store as IDs):
```json
"featuredProducts": ["product-002", "product-003"]
```

**GraphQL query** (automatic expansion):
```graphql
featuredProducts {
  items {
    ... on Product {
      id, name, price
    }
  }
}
```

**Result**: Full product objects nested inside articles, no client-side matching needed.

## Project Structure

```
src/
  ├── UmbracoCompose.Client/       # Reusable client library
  ├── UmbracoCompose.Models/       # Shared models
  └── UmbracoCompose.CLI/          # Command-line tool
examples/                          # Sample data and GraphQL queries (user-modifiable)
schemas/                           # Type schema definitions
```

## License

See [LICENSE](LICENSE) for details.
