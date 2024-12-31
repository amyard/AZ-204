using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HttpClientVersions;

public static class UsersEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        // typed httpclient
        app.MapGet("users/v4-stream/{username}", async (string username, GitHubService gitHubService, [FromServices] ILogger logger) =>
        {
            while (true)
            {
                int choice = Random.Shared.Next(0, 10);

                if (choice < 8)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    logger.LogError("Time: {time}. Invalid choice - {choice}", DateTime.UtcNow, choice);
                    throw new Exception($"Invalid choice - {choice}");
                }
                
                var content = await gitHubService.GetByUserNameAsync(username);
                logger.LogInformation("Time: {time}. User {username} logged in", DateTime.UtcNow, username);
            }

            return Results.Ok();
        });
        
        // typed httpclient
        app.MapGet("users/v4/{username}", async (string username, GitHubService gitHubService) =>
        {
            var content = await gitHubService.GetByUserNameAsync(username);

            return Results.Ok(content);
        });
        
        // named httpclientfactory
        app.MapGet("users/v3/{username}", async (string username, IHttpClientFactory factory, IOptions<GitHubSettings> settings) =>
        {
            var client = factory.CreateClient("github");
            var content = await client.GetFromJsonAsync<GitHubUser>($"users/{username}");

            return Results.Ok(content);
        });
        
        app.MapGet("users/v2/{username}", async (string username, IHttpClientFactory factory, IOptions<GitHubSettings> settings) =>
        {
            var client = factory.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", settings.Value.AccessToken);
            client.DefaultRequestHeaders.Add("User-Agent", settings.Value.UserAgent);
            client.BaseAddress = new Uri("https://api.github.com");

            var content = await client.GetFromJsonAsync<GitHubUser>($"users/{username}");

            return Results.Ok(content);
        });
        
        app.MapGet("users/v1/{username}", async (string username, IOptions<GitHubSettings> settings) =>
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", settings.Value.AccessToken);
            client.DefaultRequestHeaders.Add("User-Agent", settings.Value.UserAgent);
            client.BaseAddress = new Uri("https://api.github.com");

            var content = await client.GetFromJsonAsync<GitHubUser>($"users/{username}");

            return Results.Ok(content);
        });
    }
}
