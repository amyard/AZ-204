using HybridCacheDemo;
using Microsoft.Extensions.Caching.Hybrid;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddSingleton<WeatherService>();

// add redis cache - uncommed current state to save data into redis, and not into inmemory
// if redis is dead, it will store data into inmemory db
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});

// https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid?view=aspnetcore-9.0
#pragma warning disable EXTEXP0018
builder.Services.AddHybridCache(options =>
{
    // global statement
    options.DefaultEntryOptions = new HybridCacheEntryOptions()
    {
        LocalCacheExpiration = TimeSpan.FromMinutes(5), // for inmemory db
        Expiration = TimeSpan.FromMinutes(5) // for distributive db such as redis
    };
});
#pragma warning restore EXTEXP0018

var app = builder.Build();

app.MapGet("/weather/{city}", async (string city, WeatherService weatherService, CancellationToken cancellationToken) =>
{
    var weather = await weatherService.GetCurrentWeatherAsync(city);
    return weather is null ? Results.NotFound() : Results.Ok(weather);
})
.WithName("GetWeather")
.WithTags("Weather")
.WithOpenApi();

app.MapDelete("/weather/{city}", async (string city, HybridCache hybridCache, CancellationToken cancellationToken) =>
{
    await hybridCache.RemoveAsync($"weather:{city}", cancellationToken);
    await hybridCache.RemoveByTagAsync("weather", cancellationToken);
    await hybridCache.RemoveByTagAsync(["weather"], cancellationToken);
    
    return Results.NoContent();
})
.WithName("GetWeather")
.WithTags("Weather")
.WithOpenApi();

app.Run();
