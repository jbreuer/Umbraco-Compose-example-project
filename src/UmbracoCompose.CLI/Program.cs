using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Spectre.Console;
using UmbracoCompose.Client.Authentication;
using UmbracoCompose.Client.Ingestion;
using UmbracoCompose.Client.Management;
using UmbracoCompose.Client.Query;
using UmbracoCompose.Models;

namespace UmbracoCompose.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();
        
        // Configure services
        services.Configure<ComposeConfiguration>(configuration);
        services.AddHttpClient<IAuthenticationService, AuthenticationService>();
        services.AddHttpClient<IManagementClient, ManagementClient>();
        services.AddHttpClient<IIngestionClient, IngestionClient>();
        services.AddHttpClient<IQueryClient, QueryClient>();

        var serviceProvider = services.BuildServiceProvider();

        // Display banner
        AnsiConsole.Write(
            new FigletText("Umbraco Compose")
                .Centered()
                .Color(Color.Blue));

        // Parse command
        var command = args.Length > 0 ? args[0].ToLower() : "help";

        try
        {
            return command switch
            {
                "auth:test" => await TestAuthenticationAsync(serviceProvider),
                "setup:list" => await ListEnvironmentsAsync(serviceProvider),
                "setup:dev" => await SetupEnvironmentAsync(serviceProvider, "dev", "Development"),
                "setup:staging" => await SetupEnvironmentAsync(serviceProvider, "staging", "Staging"),
                "setup:prod" => await SetupEnvironmentAsync(serviceProvider, "prod", "Production"),
                "setup:delete" => await DeleteEnvironmentAsync(serviceProvider, args),
                "collections:list" => await ListCollectionsAsync(serviceProvider, args.Length > 1 ? args[1] : Constants.Environments.Production),
                "collections:get" => await GetCollectionDetailsAsync(serviceProvider, args),
                "collections:create" => await CreateCollectionAsync(serviceProvider, args),
                "collections:delete" => await DeleteCollectionAsync(serviceProvider, args),
                "schemas:list" => await ListTypeSchemasAsync(serviceProvider, args.Length > 1 ? args[1] : Constants.Environments.Production),
                "schemas:get" => await GetTypeSchemaAsync(serviceProvider, args),
                "schemas:create" => await CreateTypeSchemaAsync(serviceProvider, args),
                "schemas:delete" => await DeleteTypeSchemaAsync(serviceProvider, args),
                "ingest:article" => await IngestSampleArticleAsync(serviceProvider, args.Length > 1 ? args[1] : Constants.Environments.Production),
                "ingest:product" => await IngestSampleProductAsync(serviceProvider, args.Length > 1 ? args[1] : Constants.Environments.Production),
                "ingest:articles-bulk" => await IngestMultipleArticlesAsync(serviceProvider, args.Length > 1 ? args[1] : Constants.Environments.Production),
                "ingest:products-bulk" => await IngestMultipleProductsAsync(serviceProvider, args.Length > 1 ? args[1] : Constants.Environments.Production),
                "delete:article" => await DeleteArticleAsync(serviceProvider, args.Length > 1 ? args[1] : Constants.Environments.Production),
                "delete:product" => await DeleteProductAsync(serviceProvider, args.Length > 1 ? args[1] : Constants.Environments.Production),
                "delete:items-bulk" => await DeleteMultipleItemsAsync(serviceProvider, args.Length > 1 ? args[1] : Constants.Environments.Production),
                "query:articles" => await QueryArticlesAsync(serviceProvider, args.Length > 1 ? args[1] : Constants.Environments.Production),
                "query:products" => await QueryProductsAsync(serviceProvider, args.Length > 1 ? args[1] : Constants.Environments.Production),
                "query:composed" => await QueryComposedAsync(serviceProvider, args.Length > 1 ? args[1] : Constants.Environments.Production),
                "query:typed" => await QueryTypedAsync(serviceProvider, args.Length > 1 ? args[1] : Constants.Environments.Production),
                "query:articles-filtered" => await QueryArticlesFilteredAsync(serviceProvider, args.Length > 1 ? args[1] : Constants.Environments.Production),
                "query:articles-sorted" => await QueryArticlesSortedAsync(serviceProvider, args.Length > 1 ? args[1] : Constants.Environments.Production),
                "query:articles-paginated" => await QueryArticlesPaginatedAsync(serviceProvider, args),
                "query:products-filtered" => await QueryProductsFilteredAsync(serviceProvider, args.Length > 1 ? args[1] : Constants.Environments.Production),
                "query:products-sorted" => await QueryProductsSortedAsync(serviceProvider, args.Length > 1 ? args[1] : Constants.Environments.Production),
                "graphql:introspect" => await IntrospectGraphQLSchemaAsync(serviceProvider, args.Length > 1 ? args[1] : Constants.Environments.Production),
                _ => ShowHelp()
            };
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message.EscapeMarkup()}[/]");
            return 1;
        }
    }

    static bool ConfirmDeletion(string itemType, string itemName, string context)
    {
        var message = itemType switch
        {
            "collection" => $"[yellow]Are you sure you want to delete collection '{itemName}' from '{context}'? This will delete ALL content in the collection and cannot be undone.[/]",
            "environment" => $"[yellow]Are you sure you want to delete the '{itemName}' environment? This action cannot be undone.[/]",
            _ => $"[yellow]Are you sure you want to delete {itemType} '{itemName}' from '{context}'? This action cannot be undone.[/]"
        };
        
        return AnsiConsole.Confirm(message);
    }

    static string LoadGraphQLQuery(string fileName)
    {
        var queryPath = Path.Combine(AppContext.BaseDirectory, "examples", fileName);
        if (!File.Exists(queryPath))
        {
            throw new FileNotFoundException($"GraphQL query file not found: {queryPath}");
        }
        return File.ReadAllText(queryPath);
    }

    static async Task<string> LoadSchemaFileAsync(string fileName)
    {
        var schemaPath = Path.Combine(AppContext.BaseDirectory, "schemas", fileName);
        if (!File.Exists(schemaPath))
        {
            throw new FileNotFoundException($"Schema file not found: {schemaPath}");
        }
        return await File.ReadAllTextAsync(schemaPath);
    }

    static async Task<string> LoadExampleFileAsync(string fileName)
    {
        var examplePath = Path.Combine(AppContext.BaseDirectory, "examples", fileName);
        if (!File.Exists(examplePath))
        {
            throw new FileNotFoundException($"Example file not found: {examplePath}");
        }
        return await File.ReadAllTextAsync(examplePath);
    }

    static async Task<int> TestAuthenticationAsync(ServiceProvider serviceProvider)
    {
        var authService = serviceProvider.GetRequiredService<IAuthenticationService>();
        
        return await AnsiConsole.Status()
            .StartAsync("Testing authentication...", async ctx =>
            {
                var success = await authService.TestAuthenticationAsync();
                return success ? 0 : 1;
            });
    }

    static async Task<int> ListEnvironmentsAsync(ServiceProvider serviceProvider)
    {
        var managementClient = serviceProvider.GetRequiredService<IManagementClient>();
        
        return await AnsiConsole.Status()
            .StartAsync("Fetching environments...", async ctx =>
            {
                try
                {
                    var environments = await managementClient.ListEnvironmentsAsync();
                    
                    if (environments == null || environments.Count == 0)
                    {
                        AnsiConsole.MarkupLine("[yellow]No environments found.[/]");
                        return 0;
                    }
                    
                    var table = new Table();
                    table.AddColumn("Alias");
                    table.AddColumn("Description");

                    foreach (var env in environments)
                    {
                        table.AddRow(
                            env.EnvironmentAlias.EscapeMarkup(), 
                            env.Description.EscapeMarkup());
                    }

                    AnsiConsole.Write(table);
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to list environments: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> ListCollectionsAsync(ServiceProvider serviceProvider, string environmentAlias)
    {
        var managementClient = serviceProvider.GetRequiredService<IManagementClient>();
        
        return await AnsiConsole.Status()
            .StartAsync($"Fetching collections for {environmentAlias}...", async ctx =>
            {
                try
                {
                    var collections = await managementClient.ListCollectionsAsync(environmentAlias);
                    
                    if (collections == null || collections.Count == 0)
                    {
                        AnsiConsole.MarkupLine($"[yellow]No collections found in environment '{environmentAlias}'.[/]");
                        return 0;
                    }
                    
                    var table = new Table();
                    table.AddColumn("Alias");
                    table.AddColumn("Description");

                    foreach (var collection in collections)
                    {
                        table.AddRow(
                            collection.CollectionAlias.EscapeMarkup(), 
                            collection.Description.EscapeMarkup());
                    }

                    AnsiConsole.Write(table);
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to list collections: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> GetCollectionDetailsAsync(ServiceProvider serviceProvider, string[] args)
    {
        if (args.Length < 3)
        {
            AnsiConsole.MarkupLine("[red]Usage: collections:get environment collection-alias[/]");
            AnsiConsole.MarkupLine("[yellow]Example: collections:get dev articles[/]");
            return 1;
        }

        var environmentAlias = args[1];
        var collectionAlias = args[2];
        var managementClient = serviceProvider.GetRequiredService<IManagementClient>();
        
        return await AnsiConsole.Status()
            .StartAsync($"Fetching collection '{collectionAlias}' in {environmentAlias}...", async ctx =>
            {
                try
                {
                    var collection = await managementClient.GetCollectionAsync(environmentAlias, collectionAlias);
                    
                    AnsiConsole.MarkupLine($"[green]✓ Collection details retrieved![/]\n");
                    
                    var table = new Table();
                    table.AddColumn("Property");
                    table.AddColumn("Value");
                    
                    table.AddRow("[blue]Alias[/]", collection.CollectionAlias);
                    table.AddRow("[blue]Description[/]", collection.Description ?? "N/A");
                    
                    AnsiConsole.Write(table);
                    return 0;
                }
                catch (HttpRequestException ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> CreateCollectionAsync(ServiceProvider serviceProvider, string[] args)
    {
        if (args.Length < 3)
        {
            AnsiConsole.MarkupLine("[red]Usage: collections:create environment collection-alias description[/]");
            AnsiConsole.MarkupLine("[yellow]Example: collections:create production articles Articles[/]");
            return 1;
        }

        var environmentAlias = args[1];
        var collectionAlias = args[2];
        var description = args.Length > 3 ? args[3] : $"{collectionAlias} collection";

        var managementClient = serviceProvider.GetRequiredService<IManagementClient>();
        
        return await AnsiConsole.Status()
            .StartAsync($"Creating collection '{collectionAlias}' in {environmentAlias}...", async ctx =>
            {
                try
                {
                    var collection = await managementClient.CreateCollectionAsync(environmentAlias, collectionAlias, description);
                    AnsiConsole.MarkupLine($"[green]✓ Collection created: {collection.CollectionAlias}[/]");
                    AnsiConsole.MarkupLine($"  Description: {collection.Description.EscapeMarkup()}");
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to create collection: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> DeleteCollectionAsync(ServiceProvider serviceProvider, string[] args)
    {
        if (args.Length < 3)
        {
            AnsiConsole.MarkupLine("[red]Error: Environment and collection alias required[/]");
            AnsiConsole.MarkupLine("[yellow]Usage:[/] dotnet run -- collections:delete [environment] [collection-alias]");
            AnsiConsole.MarkupLine("[yellow]Example:[/] dotnet run -- collections:delete dev test-collection");
            return 1;
        }

        var environmentAlias = args[1];
        var collectionAlias = args[2];
        var managementClient = serviceProvider.GetRequiredService<IManagementClient>();
        
        // Confirm deletion
        if (!ConfirmDeletion("collection", collectionAlias, environmentAlias))
        {
            AnsiConsole.MarkupLine("[blue]Deletion cancelled.[/]");
            return 0;
        }

        AnsiConsole.MarkupLine($"[blue]Deleting collection '{collectionAlias}' from {environmentAlias}...[/]");
        
        try
        {
            await managementClient.DeleteCollectionAsync(environmentAlias, collectionAlias);
            AnsiConsole.MarkupLine($"[green]✓ Collection '{collectionAlias}' deleted successfully from {environmentAlias}[/]");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to delete collection: {ex.Message.EscapeMarkup()}[/]");
            return 1;
        }
    }

    static async Task<int> ListTypeSchemasAsync(ServiceProvider serviceProvider, string environmentAlias)
    {
        var managementClient = serviceProvider.GetRequiredService<IManagementClient>();
        
        return await AnsiConsole.Status()
            .StartAsync($"Fetching type schemas for {environmentAlias}...", async ctx =>
            {
                try
                {
                    var schemas = await managementClient.ListTypeSchemasAsync(environmentAlias);
                    
                    if (schemas == null || schemas.Count == 0)
                    {
                        AnsiConsole.MarkupLine($"[yellow]No type schemas found in environment '{environmentAlias}'.[/]");
                        return 0;
                    }
                    
                    var table = new Table();
                    table.AddColumn("Alias");
                    table.AddColumn("Name");
                    table.AddColumn("Description");

                    foreach (var schema in schemas)
                    {
                        table.AddRow(
                            schema.TypeSchemaAlias.EscapeMarkup(), 
                            schema.Name.EscapeMarkup(),
                            schema.Description.EscapeMarkup());
                    }

                    AnsiConsole.Write(table);
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to list type schemas: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> GetTypeSchemaAsync(ServiceProvider serviceProvider, string[] args)
    {
        if (args.Length < 3)
        {
            AnsiConsole.MarkupLine("[red]Usage: schemas:get environment schema-alias[/]");
            AnsiConsole.MarkupLine("[yellow]Example: schemas:get dev article[/]");
            return 1;
        }

        var environmentAlias = args[1];
        var typeSchemaAlias = args[2];
        var managementClient = serviceProvider.GetRequiredService<IManagementClient>();
        
        return await AnsiConsole.Status()
            .StartAsync($"Fetching type schema '{typeSchemaAlias}' from {environmentAlias}...", async ctx =>
            {
                try
                {
                    var schema = await managementClient.GetTypeSchemaAsync(environmentAlias, typeSchemaAlias);
                    
                    AnsiConsole.MarkupLine($"[green]✓ Type schema retrieved![/]\n");
                    
                    var table = new Table();
                    table.AddColumn("Property");
                    table.AddColumn("Value");
                    
                    table.AddRow("[blue]Alias[/]", schema.TypeSchemaAlias);
                    table.AddRow("[blue]Name[/]", schema.Name ?? "N/A");
                    table.AddRow("[blue]Description[/]", schema.Description ?? "N/A");
                    if (schema.Fields != null && schema.Fields.Count > 0)
                    {
                        table.AddRow("[blue]Fields[/]", string.Join(", ", schema.Fields.Select(f => f.Alias)));
                        table.AddRow("[blue]Field Count[/]", schema.Fields.Count.ToString());
                    }
                    
                    AnsiConsole.Write(table);
                    return 0;
                }
                catch (HttpRequestException ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> CreateTypeSchemaAsync(ServiceProvider serviceProvider, string[] args)
    {
        if (args.Length < 3)
        {
            AnsiConsole.MarkupLine("[red]Usage: schemas:create environment schema-type[/]");
            AnsiConsole.MarkupLine("[yellow]Available types: article, product[/]");
            return 1;
        }

        var environmentAlias = args[1];
        var schemaType = args[2].ToLower();

        // Determine schema file path
        var schemaFileName = schemaType switch
        {
            "article" => "article-schema-compose.json",
            "product" => "product-schema-compose.json",
            _ => null
        };

        if (schemaFileName == null)
        {
            AnsiConsole.MarkupLine($"[red]Unknown schema type: {schemaType}[/]");
            AnsiConsole.MarkupLine("[yellow]Available types: article, product[/]");
            return 1;
        }

        string schemaJson;
        try
        {
            schemaJson = await LoadSchemaFileAsync(schemaFileName);
        }
        catch (FileNotFoundException ex)
        {
            AnsiConsole.MarkupLine($"[red]{ex.Message.EscapeMarkup()}[/]");
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to read schema file: {ex.Message.EscapeMarkup()}[/]");
            return 1;
        }

        var managementClient = serviceProvider.GetRequiredService<IManagementClient>();
        
        return await AnsiConsole.Status()
            .StartAsync($"Creating {schemaType} schema in {environmentAlias}...", async ctx =>
            {
                try
                {
                    var result = await managementClient.CreateTypeSchemaAsync(environmentAlias, schemaJson);
                    AnsiConsole.MarkupLine($"[green]✓ Type schema created: {result.TypeSchemaAlias}[/]");
                    AnsiConsole.MarkupLine($"  Name: {result.Name.EscapeMarkup()}");
                    if (result.Fields != null && result.Fields.Count > 0)
                    {
                        AnsiConsole.MarkupLine($"  Fields: {result.Fields.Count}");
                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to create type schema: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> DeleteTypeSchemaAsync(ServiceProvider serviceProvider, string[] args)
    {
        if (args.Length < 3)
        {
            AnsiConsole.MarkupLine("[red]Error: Environment and schema alias required[/]");
            AnsiConsole.MarkupLine("[yellow]Usage:[/] dotnet run -- schemas:delete [environment] [schema-alias]");
            AnsiConsole.MarkupLine("[yellow]Example:[/] dotnet run -- schemas:delete dev article");
            return 1;
        }

        var environmentAlias = args[1];
        var typeSchemaAlias = args[2];
        var managementClient = serviceProvider.GetRequiredService<IManagementClient>();
        
        // Confirm deletion
        if (!ConfirmDeletion("type schema", typeSchemaAlias, environmentAlias))
        {
            AnsiConsole.MarkupLine("[blue]Deletion cancelled.[/]");
            return 0;
        }

        AnsiConsole.MarkupLine($"[blue]Deleting type schema '{typeSchemaAlias}' from {environmentAlias}...[/]");
        
        try
        {
            await managementClient.DeleteTypeSchemaAsync(environmentAlias, typeSchemaAlias);
            AnsiConsole.MarkupLine($"[green]✓ Type schema '{typeSchemaAlias}' deleted successfully from {environmentAlias}[/]");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to delete type schema: {ex.Message.EscapeMarkup()}[/]");
            return 1;
        }
    }

    static async Task<int> SetupEnvironmentAsync(ServiceProvider serviceProvider, string alias, string description)
    {
        var managementClient = serviceProvider.GetRequiredService<IManagementClient>();
        
        AnsiConsole.MarkupLine($"[blue]Setting up {description} environment...[/]");

        // Create environment
        var env = await managementClient.CreateEnvironmentAsync(alias, description);
        AnsiConsole.MarkupLine($"[green]✓[/] Environment created: {env.EnvironmentAlias}");

        AnsiConsole.MarkupLine($"\n[green]✓ {description} environment setup complete![/]");
        AnsiConsole.MarkupLine($"[yellow]Note:[/] Using Personal Access Token is recommended for automatic cross-environment access.");
        return 0;
    }

    static async Task<int> DeleteEnvironmentAsync(ServiceProvider serviceProvider, string[] args)
    {
        if (args.Length < 2)
        {
            AnsiConsole.MarkupLine("[red]Error: Environment alias required[/]");
            AnsiConsole.MarkupLine("[yellow]Usage:[/] dotnet run -- setup:delete [environment-alias]");
            AnsiConsole.MarkupLine("[yellow]Example:[/] dotnet run -- setup:delete dev");
            return 1;
        }

        var environmentAlias = args[1];
        var managementClient = serviceProvider.GetRequiredService<IManagementClient>();
        
        // Confirm deletion
        if (!ConfirmDeletion("environment", environmentAlias, ""))
        {
            AnsiConsole.MarkupLine("[blue]Deletion cancelled.[/]");
            return 0;
        }

        AnsiConsole.MarkupLine($"[blue]Deleting environment '{environmentAlias}'...[/]");
        
        await managementClient.DeleteEnvironmentAsync(environmentAlias);
        AnsiConsole.MarkupLine($"[green]✓ Environment '{environmentAlias}' deleted successfully[/]");
        
        return 0;
    }

    static async Task<int> IngestSampleArticleAsync(ServiceProvider serviceProvider, string environment = Constants.Environments.Production)
    {
        var ingestionClient = serviceProvider.GetRequiredService<IIngestionClient>();
        
        string jsonPayload;
        try
        {
            jsonPayload = await LoadExampleFileAsync("ingest-article.json");
        }
        catch (FileNotFoundException ex)
        {
            AnsiConsole.MarkupLine($"[red]{ex.Message.EscapeMarkup()}[/]");
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to read file: {ex.Message.EscapeMarkup()}[/]");
            return 1;
        }

        return await AnsiConsole.Status()
            .StartAsync($"Ingesting article to {environment}...", async ctx =>
            {
                try
                {
                    await ingestionClient.IngestJsonAsync(environment, "articles", jsonPayload);
                    AnsiConsole.MarkupLine($"[green]✓ Article ingested successfully to {environment}![/]");
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> IngestSampleProductAsync(ServiceProvider serviceProvider, string environment = Constants.Environments.Production)
    {
        var ingestionClient = serviceProvider.GetRequiredService<IIngestionClient>();
        
        string jsonPayload;
        try
        {
            jsonPayload = await LoadExampleFileAsync("ingest-product.json");
        }
        catch (FileNotFoundException ex)
        {
            AnsiConsole.MarkupLine($"[red]{ex.Message.EscapeMarkup()}[/]");
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to read file: {ex.Message.EscapeMarkup()}[/]");
            return 1;
        }

        return await AnsiConsole.Status()
            .StartAsync($"Ingesting product to {environment}...", async ctx =>
            {
                try
                {
                    await ingestionClient.IngestJsonAsync(environment, "products", jsonPayload);
                    AnsiConsole.MarkupLine($"[green]✓ Product ingested successfully to {environment}![/]");
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> IngestMultipleArticlesAsync(ServiceProvider serviceProvider, string environment = Constants.Environments.Production)
    {
        var ingestionClient = serviceProvider.GetRequiredService<IIngestionClient>();
        
        string jsonPayload;
        try
        {
            jsonPayload = await LoadExampleFileAsync("ingest-multiple-articles.json");
        }
        catch (FileNotFoundException ex)
        {
            AnsiConsole.MarkupLine($"[red]{ex.Message.EscapeMarkup()}[/]");
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to read file: {ex.Message.EscapeMarkup()}[/]");
            return 1;
        }

        return await AnsiConsole.Status()
            .StartAsync($"Ingesting 10 articles to {environment}...", async ctx =>
            {
                try
                {
                    await ingestionClient.IngestJsonAsync(environment, "articles", jsonPayload);
                    AnsiConsole.MarkupLine($"[green]✓ 10 articles ingested successfully to {environment}![/]");
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> IngestMultipleProductsAsync(ServiceProvider serviceProvider, string environment = Constants.Environments.Production)
    {
        var ingestionClient = serviceProvider.GetRequiredService<IIngestionClient>();
        
        string jsonPayload;
        try
        {
            jsonPayload = await LoadExampleFileAsync("ingest-multiple-products.json");
        }
        catch (FileNotFoundException ex)
        {
            AnsiConsole.MarkupLine($"[red]{ex.Message.EscapeMarkup()}[/]");
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to read file: {ex.Message.EscapeMarkup()}[/]");
            return 1;
        }

        return await AnsiConsole.Status()
            .StartAsync($"Ingesting 10 products to {environment}...", async ctx =>
            {
                try
                {
                    await ingestionClient.IngestJsonAsync(environment, "products", jsonPayload);
                    AnsiConsole.MarkupLine($"[green]✓ 10 products ingested successfully to {environment}![/]");
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> DeleteArticleAsync(ServiceProvider serviceProvider, string environment = Constants.Environments.Production)
    {
        var ingestionClient = serviceProvider.GetRequiredService<IIngestionClient>();
        
        string jsonPayload;
        try
        {
            jsonPayload = await LoadExampleFileAsync("delete-article.json");
        }
        catch (FileNotFoundException ex)
        {
            AnsiConsole.MarkupLine($"[red]{ex.Message.EscapeMarkup()}[/]");
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to read file: {ex.Message.EscapeMarkup()}[/]");
            return 1;
        }

        return await AnsiConsole.Status()
            .StartAsync($"Deleting article from {environment}...", async ctx =>
            {
                try
                {
                    await ingestionClient.IngestJsonAsync(environment, "articles", jsonPayload);
                    AnsiConsole.MarkupLine($"[green]✓ Article deleted successfully from {environment}![/]");
                    AnsiConsole.MarkupLine($"[yellow]Note:[/] Deleted article with ID 'article-001'");
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> DeleteProductAsync(ServiceProvider serviceProvider, string environment = Constants.Environments.Production)
    {
        var ingestionClient = serviceProvider.GetRequiredService<IIngestionClient>();
        
        string jsonPayload;
        try
        {
            jsonPayload = await LoadExampleFileAsync("delete-product.json");
        }
        catch (FileNotFoundException ex)
        {
            AnsiConsole.MarkupLine($"[red]{ex.Message.EscapeMarkup()}[/]");
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to read file: {ex.Message.EscapeMarkup()}[/]");
            return 1;
        }

        return await AnsiConsole.Status()
            .StartAsync($"Deleting product from {environment}...", async ctx =>
            {
                try
                {
                    await ingestionClient.IngestJsonAsync(environment, "products", jsonPayload);
                    AnsiConsole.MarkupLine($"[green]✓ Product deleted successfully from {environment}![/]");
                    AnsiConsole.MarkupLine($"[yellow]Note:[/] Deleted product with ID 'product-001'");
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> DeleteMultipleItemsAsync(ServiceProvider serviceProvider, string environment = Constants.Environments.Production)
    {
        var ingestionClient = serviceProvider.GetRequiredService<IIngestionClient>();
        
        string jsonPayload;
        try
        {
            jsonPayload = await LoadExampleFileAsync("delete-multiple-items.json");
        }
        catch (FileNotFoundException ex)
        {
            AnsiConsole.MarkupLine($"[red]{ex.Message.EscapeMarkup()}[/]");
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to read file: {ex.Message.EscapeMarkup()}[/]");
            return 1;
        }

        return await AnsiConsole.Status()
            .StartAsync($"Deleting multiple items from {environment}...", async ctx =>
            {
                try
                {
                    // Parse the JSON to separate items by collection
                    var items = System.Text.Json.JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(jsonPayload);
                    if (items == null || items.Count == 0)
                    {
                        AnsiConsole.MarkupLine("[yellow]No items to delete.[/]");
                        return 0;
                    }

                    // Group by type/collection and delete separately
                    var articleItems = items.Where(i => i.GetProperty("id").GetString()?.StartsWith("article-") == true).ToList();
                    var productItems = items.Where(i => i.GetProperty("id").GetString()?.StartsWith("product-") == true).ToList();

                    if (articleItems.Any())
                    {
                        var articlesJson = System.Text.Json.JsonSerializer.Serialize(articleItems);
                        await ingestionClient.IngestJsonAsync(environment, Constants.Collections.Articles, articlesJson);
                        AnsiConsole.MarkupLine($"[green]✓[/] Deleted {articleItems.Count} article(s) from {environment}");
                    }

                    if (productItems.Any())
                    {
                        var productsJson = System.Text.Json.JsonSerializer.Serialize(productItems);
                        await ingestionClient.IngestJsonAsync(environment, Constants.Collections.Products, productsJson);
                        AnsiConsole.MarkupLine($"[green]✓[/] Deleted {productItems.Count} product(s) from {environment}");
                    }

                    AnsiConsole.MarkupLine($"[green]✓ All items deleted successfully from {environment}![/]");
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> QueryArticlesAsync(ServiceProvider serviceProvider, string environment = Constants.Environments.Production)
    {
        var queryClient = serviceProvider.GetRequiredService<IQueryClient>();
        var query = LoadGraphQLQuery("query-articles.graphql");

        return await AnsiConsole.Status()
            .StartAsync($"Querying articles from {environment}...", async ctx =>
            {
                try
                {
                    var result = await queryClient.ExecuteQueryAsJsonAsync(query, environment: environment);
                    
                    if (result == null)
                    {
                        AnsiConsole.MarkupLine("[yellow]No data returned.[/]");
                        return 1;
                    }

                    AnsiConsole.MarkupLine("[green]✓ Query successful![/]");
                    AnsiConsole.WriteLine(System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> QueryProductsAsync(ServiceProvider serviceProvider, string environment = Constants.Environments.Production)
    {
        var queryClient = serviceProvider.GetRequiredService<IQueryClient>();
        var query = LoadGraphQLQuery("query-products.graphql");

        return await AnsiConsole.Status()
            .StartAsync($"Querying products from {environment}...", async ctx =>
            {
                try
                {
                    var result = await queryClient.ExecuteQueryAsJsonAsync(query, environment: environment);
                    
                    if (result == null)
                    {
                        AnsiConsole.MarkupLine("[yellow]No data returned.[/]");
                        return 1;
                    }

                    AnsiConsole.MarkupLine("[green]✓ Query successful![/]");
                    AnsiConsole.WriteLine(System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> QueryComposedAsync(ServiceProvider serviceProvider, string environment = Constants.Environments.Production)
    {
        var queryClient = serviceProvider.GetRequiredService<IQueryClient>();
        var query = LoadGraphQLQuery("query-composed.graphql");

        return await AnsiConsole.Status()
            .StartAsync($"Querying unified content (articles + products) from {environment}...", async ctx =>
            {
                var result = await queryClient.ExecuteQueryAsync<dynamic>(query, environment: environment);
                
                if (result.Errors != null && result.Errors.Any())
                {
                    foreach (var error in result.Errors)
                    {
                        AnsiConsole.MarkupLine($"[red]Error: {error.Message}[/]");
                    }
                    return 1;
                }

                AnsiConsole.MarkupLine("[green]✓ Query successful![/]");
                AnsiConsole.MarkupLine("[blue]This demonstrates Compose's core value: automatic content expansion from multiple sources[/]");
                AnsiConsole.MarkupLine("[yellow]→ Products automatically nested inside articles - no client-side matching needed![/]");
                AnsiConsole.MarkupLine("[dim]Articles from one source + Products from another = Unified in ONE query[/]\n");
                AnsiConsole.WriteLine(System.Text.Json.JsonSerializer.Serialize(result.Data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                return 0;
            });
    }

    static async Task<int> QueryTypedAsync(ServiceProvider serviceProvider, string environment = Constants.Environments.Production)
    {
        var queryClient = serviceProvider.GetRequiredService<IQueryClient>();
        var query = LoadGraphQLQuery("query-composed.graphql");

        return await AnsiConsole.Status()
            .StartAsync($"Querying with strongly-typed models from {environment}...", async ctx =>
            {
                // Use strongly-typed response model instead of dynamic
                var result = await queryClient.ExecuteQueryAsync<GraphQLResponseModels.ComposedQueryResponse>(
                    query, 
                    environment: environment);
                
                if (result.Errors != null && result.Errors.Any())
                {
                    foreach (var error in result.Errors)
                    {
                        AnsiConsole.MarkupLine($"[red]Error: {error.Message}[/]");
                    }
                    return 1;
                }

                if (result.Data?.Articles?.Items == null || !result.Data.Articles.Items.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No articles found.[/]");
                    return 0;
                }

                AnsiConsole.MarkupLine("[green]✓ Query successful with strongly-typed models![/]");
                AnsiConsole.MarkupLine("[blue]Demonstrating type-safe access to query results[/]\n");

                // Create a table to display the strongly-typed results
                var table = new Table();
                table.Border(TableBorder.Rounded);
                table.AddColumn("[yellow]Article[/]");
                table.AddColumn("[cyan]Author[/]");
                table.AddColumn("[green]Featured Products[/]");

                foreach (var article in result.Data.Articles.Items)
                {
                    var productNames = article.FeaturedProducts?.Items
                        .Select(p => $"{p.Name} (${p.Price})")
                        .ToList() ?? new List<string>();

                    var productsText = productNames.Any() 
                        ? string.Join("\n", productNames)
                        : "[dim]No featured products[/]";

                    table.AddRow(
                        $"[bold]{article.Title.EscapeMarkup()}[/]\n[dim]{article.Slug}[/]",
                        article.Author.EscapeMarkup(),
                        productsText);
                }

                AnsiConsole.Write(table);

                // Show some type-safe operations
                var articlesWithProducts = result.Data.Articles.Items
                    .Where(a => a.FeaturedProducts?.Items.Any() == true)
                    .ToList();

                if (articlesWithProducts.Any())
                {
                    AnsiConsole.MarkupLine($"\n[cyan]Type-safe analysis:[/]");
                    AnsiConsole.MarkupLine($"  • Total articles: {result.Data.Articles.Items.Count}");
                    AnsiConsole.MarkupLine($"  • Articles with featured products: {articlesWithProducts.Count}");
                    
                    var totalProducts = articlesWithProducts
                        .Sum(a => a.FeaturedProducts?.Items.Count ?? 0);
                    AnsiConsole.MarkupLine($"  • Total featured products: {totalProducts}");

                    var avgPrice = articlesWithProducts
                        .SelectMany(a => a.FeaturedProducts?.Items ?? new List<GraphQLResponseModels.ProductNode>())
                        .Average(p => p.Price);
                    AnsiConsole.MarkupLine($"  • Average product price: ${avgPrice:F2}");
                }

                AnsiConsole.MarkupLine($"\n[dim]💡 Tip: Using strongly-typed models provides IntelliSense, compile-time safety, and enables LINQ queries![/]");
                
                return 0;
            });
    }

    static async Task<int> QueryArticlesFilteredAsync(ServiceProvider serviceProvider, string environment = Constants.Environments.Production)
    {
        var queryClient = serviceProvider.GetRequiredService<IQueryClient>();
        var query = LoadGraphQLQuery("query-articles-filtered.graphql");

        return await AnsiConsole.Status()
            .StartAsync($"Querying published articles from {environment}...", async ctx =>
            {
                try
                {
                    var result = await queryClient.ExecuteQueryAsJsonAsync(query, environment: environment);
                    
                    if (result == null)
                    {
                        AnsiConsole.MarkupLine("[yellow]No data returned.[/]");
                        return 1;
                    }

                    AnsiConsole.MarkupLine("[green]✓ Query successful (filtered by status = published)![/]");
                    AnsiConsole.WriteLine(System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> QueryArticlesSortedAsync(ServiceProvider serviceProvider, string environment = Constants.Environments.Production)
    {
        var queryClient = serviceProvider.GetRequiredService<IQueryClient>();
        var query = LoadGraphQLQuery("query-articles-sorted.graphql");

        return await AnsiConsole.Status()
            .StartAsync($"Querying articles (sorted by date) from {environment}...", async ctx =>
            {
                try
                {
                    var result = await queryClient.ExecuteQueryAsJsonAsync(query, environment: environment);
                    
                    if (result == null)
                    {
                        AnsiConsole.MarkupLine("[yellow]No data returned.[/]");
                        return 1;
                    }

                    AnsiConsole.MarkupLine("[green]✓ Query successful (sorted by publishedDate DESC)![/]");
                    AnsiConsole.WriteLine(System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> QueryArticlesPaginatedAsync(ServiceProvider serviceProvider, string[] args)
    {
        var environment = args.Length > 1 ? args[1] : Constants.Environments.Production;
        var pageSize = args.Length > 2 && int.TryParse(args[2], out var size) ? size : 2;
        
        var queryClient = serviceProvider.GetRequiredService<IQueryClient>();
        var query = LoadGraphQLQuery("query-articles-paginated.graphql");

        var variables = new Dictionary<string, object>
        {
            { "first", pageSize },
            { "after", null! }
        };

        return await AnsiConsole.Status()
            .StartAsync($"Querying articles (first {pageSize}) from {environment}...", async ctx =>
            {
                try
                {
                    var result = await queryClient.ExecuteQueryAsJsonAsync(query, variables, environment);
                    
                    if (result == null)
                    {
                        AnsiConsole.MarkupLine("[yellow]No data returned.[/]");
                        return 1;
                    }

                    AnsiConsole.MarkupLine($"[green]✓ Query successful (first {pageSize} items with pagination info)![/]");
                    AnsiConsole.WriteLine(System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> QueryProductsFilteredAsync(ServiceProvider serviceProvider, string environment = Constants.Environments.Production)
    {
        var queryClient = serviceProvider.GetRequiredService<IQueryClient>();
        var query = LoadGraphQLQuery("query-products-filtered.graphql");

        return await AnsiConsole.Status()
            .StartAsync($"Querying active products under $100 from {environment}...", async ctx =>
            {
                try
                {
                    var result = await queryClient.ExecuteQueryAsJsonAsync(query, environment: environment);
                    
                    if (result == null)
                    {
                        AnsiConsole.MarkupLine("[yellow]No data returned.[/]");
                        return 1;
                    }

                    AnsiConsole.MarkupLine("[green]✓ Query successful (filtered by price <= 100 and isActive = true)![/]");
                    AnsiConsole.WriteLine(System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> QueryProductsSortedAsync(ServiceProvider serviceProvider, string environment = Constants.Environments.Production)
    {
        var queryClient = serviceProvider.GetRequiredService<IQueryClient>();
        var query = LoadGraphQLQuery("query-products-sorted.graphql");

        return await AnsiConsole.Status()
            .StartAsync($"Querying products (sorted by price) from {environment}...", async ctx =>
            {
                try
                {
                    var result = await queryClient.ExecuteQueryAsJsonAsync(query, environment: environment);
                    
                    if (result == null)
                    {
                        AnsiConsole.MarkupLine("[yellow]No data returned.[/]");
                        return 1;
                    }

                    AnsiConsole.MarkupLine("[green]✓ Query successful (sorted by price ASC)![/]");
                    AnsiConsole.WriteLine(System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static async Task<int> IntrospectGraphQLSchemaAsync(ServiceProvider serviceProvider, string environment = Constants.Environments.Production)
    {
        var queryClient = serviceProvider.GetRequiredService<IQueryClient>();
        var query = LoadGraphQLQuery("graphql-introspect.graphql");

        return await AnsiConsole.Status()
            .StartAsync($"Running GraphQL introspection on {environment}...", async ctx =>
            {
                try
                {
                    var result = await queryClient.ExecuteQueryAsJsonAsync(query, environment: environment);
                    
                    if (result == null)
                    {
                        AnsiConsole.MarkupLine("[yellow]No data returned.[/]");
                        return 1;
                    }

                    AnsiConsole.MarkupLine("[green]✓ Introspection successful![/]");
                    AnsiConsole.WriteLine(System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message.EscapeMarkup()}[/]");
                    return 1;
                }
            });
    }

    static int ShowHelp()
    {
        var table = new Table();
        table.AddColumn("Command");
        table.AddColumn("Description");

        AnsiConsole.MarkupLine("[bold]Umbraco Compose CLI - Available Commands[/]\n");
        
        table.AddRow("[blue]auth:test[/]", "Test OAuth authentication");
        table.AddEmptyRow();
        
        table.AddRow("[yellow]Environment Setup[/]", "");
        table.AddRow("[blue]setup:list[/]", "List all environments");
        table.AddRow("[blue]setup:dev[/]", "Create dev environment");
        table.AddRow("[blue]setup:staging[/]", "Create staging environment");
        table.AddRow("[blue]setup:prod[/]", "Create production environment");
        table.AddRow("[blue]setup:delete[/]", "Delete environment [[env-alias]]");
        table.AddEmptyRow();
        
        table.AddRow("[yellow]Collections[/]", "");
        table.AddRow("[blue]collections:list[/]", "List collections [[env]] (default: production)");
        table.AddRow("[blue]collections:get[/]", "Get collection details [[env]] [[alias]]");
        table.AddRow("[blue]collections:create[/]", "Create collection [[env]] [[alias]] [[desc]]");
        table.AddRow("[blue]collections:delete[/]", "Delete collection [[env]] [[alias]] (with confirmation)");
        table.AddEmptyRow();
        
        table.AddRow("[yellow]Type Schemas[/]", "");
        table.AddRow("[blue]schemas:list[/]", "List type schemas [[env]] (default: production)");
        table.AddRow("[blue]schemas:get[/]", "Get schema details [[env]] [[alias]]");
        table.AddRow("[blue]schemas:create[/]", "Create schema [[env]] [[type]]");
        table.AddRow("[blue]schemas:delete[/]", "Delete schema [[env]] [[alias]] (with confirmation)");
        table.AddEmptyRow();
        
        table.AddRow("[yellow]Content Ingestion[/]", "");
        table.AddRow("[blue]ingest:article[/]", "Ingest sample article [[env]] (default: production)");
        table.AddRow("[blue]ingest:product[/]", "Ingest sample product [[env]] (default: production)");
        table.AddRow("[blue]ingest:articles-bulk[/]", "Ingest 10 sample articles [[env]] (default: production)");
        table.AddRow("[blue]ingest:products-bulk[/]", "Ingest 10 sample products [[env]] (default: production)");
        table.AddEmptyRow();
        
        table.AddRow("[yellow]Content Deletion[/]", "");
        table.AddRow("[blue]delete:article[/]", "Delete sample article (article-001) [[env]]");
        table.AddRow("[blue]delete:product[/]", "Delete sample product (product-001) [[env]]");
        table.AddRow("[blue]delete:items-bulk[/]", "Delete multiple items [[env]]");
        table.AddEmptyRow();
        
        table.AddRow("[yellow]GraphQL Queries[/]", "");
        table.AddRow("[blue]query:articles[/]", "Query all articles [[env]] (default: production)");
        table.AddRow("[blue]query:products[/]", "Query all products [[env]] (default: production)");
        table.AddRow("[blue]query:composed[/]", "Query articles with auto-nested products [[env]]");
        table.AddRow("[blue]query:typed[/]", "Query with strongly-typed models [[env]] (example)");
        table.AddRow("[blue]query:articles-filtered[/]", "Query published articles [[env]]");
        table.AddRow("[blue]query:articles-sorted[/]", "Query articles sorted by date [[env]]");
        table.AddRow("[blue]query:articles-paginated[/]", "Query articles with pagination [[env]] [[pageSize]]");
        table.AddRow("[blue]query:products-filtered[/]", "Query active products under $100 [[env]]");
        table.AddRow("[blue]query:products-sorted[/]", "Query products sorted by price [[env]]");
        table.AddRow("[blue]graphql:introspect[/]", "Inspect GraphQL schema (NodeFilterInput) [[env]]");

        AnsiConsole.Write(table);
        return 0;
    }
}
