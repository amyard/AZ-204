using Dapper;
using Npgsql;

namespace UrlShortener.Api.Services;

public class DatabaseInitializer(
    NpgsqlDataSource dataSource,
    IConfiguration configuration,
    ILogger<DatabaseInitializer> logger
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await CreateDatabaseIfNotExists(stoppingToken);
            await InitializeSchema(stoppingToken);
            logger.LogInformation("Database created");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing database");
            throw;
        }
    }

    private async Task CreateDatabaseIfNotExists(CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("url-shortener");
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        string? databaseName = builder.Database;
        builder.Database = "postgres";
        
        await using var connection = new NpgsqlConnection(builder.ToString());
        await connection.OpenAsync(cancellationToken);

        bool dataBaseExists = await connection.ExecuteScalarAsync<bool>(
            "SELECT EXISTS (SELECT 1 FROM pd_database WHERE database_name = @databaseName)",
            new {databaseName});

        if (!dataBaseExists)
        {
            logger.LogInformation("Creating database '{DatabaseName}'", databaseName);
            await connection.ExecuteAsync($"CREATE DATABASE {databaseName}");
        }
    }

    private async Task InitializeSchema(CancellationToken cancellationToken)
    {
        const string createTableSql = 
            """
            CREATE TABLE IF NOT EXISTS shortened_urls (
                id Serial NOT NULL PRIMARY KEY,
                short_code varchar(10) UNIQUE NOT NULL,
                original_url TEXT NOT NULL,
                created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
            );
            CREATE INDEX IF NOT EXISTS idx_short_code ON shortened_urls (short_code);

            CREATE TABLE IF NOT EXISTS url_visits (
                id serial NOT NULL PRIMARY KEY,
                short_code varchar(10) NOT NULL,
                created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
                user_agent TEXT,
                referer TEXT,
                FOREIGN KEY (short_code) REFERENCES shortened_urls (short_code)
            );
            CREATE INDEX IF NOT EXISTS idx_visits_short_code ON url_visits (short_code);
            """;
        
        await using var command = dataSource.CreateCommand(createTableSql);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
