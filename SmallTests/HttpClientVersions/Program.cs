using HttpClientVersions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddOptions<GitHubSettings>()
    .BindConfiguration(GitHubSettings.ConfigurationSection)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient("github", (serviceProvider, httpClient) =>
{
    var gitHubSettings = serviceProvider.GetRequiredService<IOptions<GitHubSettings>>().Value;
    
    httpClient.DefaultRequestHeaders.Add("Authorization", gitHubSettings.AccessToken);
    httpClient.DefaultRequestHeaders.Add("User-Agent", gitHubSettings.UserAgent);
    httpClient.BaseAddress = new Uri("https://api.github.com");
});

// Typed httpClient v4
// register as Transient in DI
builder.Services.AddHttpClient<GitHubService>((serviceProvider, httpClient) =>
{
    var gitHubSettings = serviceProvider.GetRequiredService<IOptions<GitHubSettings>>().Value;

    httpClient.DefaultRequestHeaders.Add("Authorization", gitHubSettings.AccessToken);
    httpClient.DefaultRequestHeaders.Add("User-Agent", gitHubSettings.UserAgent);
    httpClient.BaseAddress = new Uri("https://api.github.com");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new SocketsHttpHandler
    {
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5)
    };
})
.SetHandlerLifetime(Timeout.InfiniteTimeSpan)
// .AddResilienceHandler("custom", pipeline =>
// {
//     pipeline.AddRetry(new HttpRetryStrategyOptions
//     {
//         MaxRetryAttempts = 3,
//         BackoffType = DelayBackoffType.Exponential, // every subsequence retry will have larger delay value
//         UseJitter = true,
//         Delay = TimeSpan.FromMilliseconds(500)
//     });
//
//     // if during 10 sec we fail 90% of request, we will prevent sending requests for 5 sec.
//     // after 5 sec we will make test call to check the downtown stream
//     pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
//     {
//         SamplingDuration = TimeSpan.FromSeconds(10),
//         FailureRatio = 0.9,
//         MinimumThroughput = 5,
//         BreakDuration = TimeSpan.FromSeconds(5),
//     });
//     
//     // timeout for per request
//     pipeline.AddTimeout(TimeSpan.FromSeconds(1)); 
// })
.AddStandardResilienceHandler();  // default resilient template handler. the same code as above commented 

builder.Services.AddSingleton<ILogger>(sp =>
{
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return loggerFactory.CreateLogger("DefaultLogger");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapUserEndpoints();

app.Run();
