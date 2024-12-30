using UrlShortener.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDataSource("url-shortener");
builder.AddRedisDistributedCache("redis");

#pragma warning disable EXTEXP0018
builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018

builder.Services.AddOpenApi();
builder.Services.AddHostedService<DatabaseInitializer>();
builder.Services.AddScoped<UrlShorteningService>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("shorten", async (string url, UrlShorteningService urlShorteningService) =>
{
    if (!Uri.TryCreate(url, UriKind.Absolute, out _))
    {
        return Results.BadRequest("Invalid url format");
    }

    var shortCode = await urlShorteningService.ShortenUrl(url);
    
    return Results.Ok(shortCode);
});

app.MapGet("{shortCode}", async (string shortCode, UrlShorteningService urlShorteningService) =>
{
    var originalUrl = await urlShorteningService.GetOriginalUrl(shortCode);
    return originalUrl is not null ? Results.Redirect(originalUrl) : Results.NotFound();
});

app.MapGet("urls", async (UrlShorteningService urlShorteningService) =>
{
    return Results.Ok(await urlShorteningService.GetAllUrls());
});

app.UseHttpsRedirection();

app.Run();
