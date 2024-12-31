namespace HttpClientVersions;

public class GitHubSettings
{
    public const string ConfigurationSection = "GitHub";
    
    public required string AccessToken { get; init; } = string.Empty;
    
    public required string UserAgent { get; init; } = string.Empty;
}
