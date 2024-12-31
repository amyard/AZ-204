using System.ComponentModel.DataAnnotations;

namespace InputValidation.Models;

public class GitHubSettings
{
    public const string ConfigurationSection = "GitHub";
    
    public required string AccessToken { get; init; } = string.Empty;
    
    public required string UserAgent { get; init; } = string.Empty;
    
    [Required, Url]
    public string BaseAddress { get; init; } = string.Empty;
}
