using Microsoft.Extensions.Caching.Hybrid;

namespace HybridCacheDemo;

public class WeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HybridCache _hybridCache;

    public WeatherService(IHttpClientFactory httpClientFactory, HybridCache hybridCache)
    {
        _httpClientFactory = httpClientFactory;
        _hybridCache = hybridCache;
    }

    public async Task<WeatherResponse?> GetCurrentWeatherAsync(string city, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"weather:{city}";
        var weather = await _hybridCache.GetOrCreateAsync<WeatherResponse?>(cacheKey,
            async _ => await GetWeatherAsync(city),
            new HybridCacheEntryOptions
            {
                // local changes for current cache
                LocalCacheExpiration = TimeSpan.FromMinutes(10),
                Flags = HybridCacheEntryFlags.None
            },
            tags: ["weather"],
            cancellationToken: cancellationToken);
        
        return weather;
    }

    public async Task<WeatherResponse?> GetWeatherAsync(string city)
    {
        string oldKey = "25d1f014ad336f93db471b48bfb5d20c";
        string newKey = "629f79f2790422cb006e0180c0fa905f";
        var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={oldKey}";
        var httpClient = _httpClientFactory.CreateClient();
        var weatherResponse = await httpClient.GetAsync(url);

        if (weatherResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        var res = await weatherResponse.Content.ReadFromJsonAsync<WeatherResponse>();

        return res;
    }
}
