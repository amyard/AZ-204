using Dapper;
using Microsoft.Extensions.Caching.Hybrid;
using Npgsql;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Services;

internal sealed class UrlShorteningService(
    NpgsqlDataSource dataSource, 
    HybridCache hybridCache,
    ILogger<UrlShorteningService> logger)
{
    private const int MaxRetries = 3;
    
    public async Task<string> ShortenUrl(string url)
    {
        for (var attemp = 1; attemp <= MaxRetries; attemp++)
        {
            try
            {
                var shortCode = GenerateShortCode();

                const string sql =
                    """
                    INSERT INTO shortened_urls(short_code, original_urls) 
                    VALUES(@ShortCode, @OriginalUrl)
                    RETURNING short_code;
                    """;

                await using var connection = await dataSource.OpenConnectionAsync();
                var result = await connection.QuerySingleAsync<string>(
                    sql,
                    new { ShortCode = shortCode, OriginalUrl = url });
                
                await hybridCache.SetAsync(shortCode, url);

                return result;
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                if (attemp == MaxRetries)
                {
                    logger.LogError(
                        ex,
                        "Failed to generate unique short code after {MaxRetries} attempts",
                        MaxRetries);
                    
                    throw new InvalidOperationException("Failed to generate unique short code", ex);
                }
                
                logger.LogWarning(
                    "Short code collision occured. Retrying ... (Attempt {Attemp} of {MaxRetries})",
                    attemp + 1, MaxRetries);
            }
        }
        
        throw new InvalidOperationException("Failed to generate unique short code");
    }

    private static string GenerateShortCode()
    {
        const int length = 7;
        const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        
        var chars = Enumerable.Range(0, length).Select(_ => alphabet[Random.Shared.Next(alphabet.Length)]);
        
        return new string(chars.ToArray());
    }

    public async Task<string?> GetOriginalUrl(string shortCode)
    {
        var originalUrls = await hybridCache.GetOrCreateAsync(shortCode, async token =>
        {
            const string sql = 
                """
                SELECT original_url
                FROM shortened_urls
                where short_code = @ShortCode;
                """;
        
            await using var connection = await dataSource.OpenConnectionAsync(token);
            var originalUrls = await connection.QuerySingleOrDefaultAsync<string>(
                sql,
                new { ShortCode = shortCode });
        
            return originalUrls;
        });
        
        return originalUrls;
    }

    public async Task<IEnumerable<ShortenedUrl>> GetAllUrls()
    {
        const string sql = 
            """
            SELECT short_code as ShortCode, original_url as OriginalUrl, created_at as CreatedAt
            FROM shortened_urls
            ORDER BY created_at ASC";"
            """;
        
        await using var connection = await dataSource.OpenConnectionAsync();

        return await connection.QueryAsync<ShortenedUrl>(sql);
    }
}
