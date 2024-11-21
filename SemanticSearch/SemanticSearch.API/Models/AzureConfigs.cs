namespace SemanticSearch.API.Models;

public class AzureConfigs
{
    public string SemanticSearchAPIKey { get; init; } = string.Empty;
    public string SemanticSearchEndpoint { get; init; } = string.Empty;
    public string SemanticSearchIndexNameProdEnv { get; init; } = string.Empty;
    public string SemanticSearchIndexNameTestEnv { get; init; } = string.Empty;
    public string SemanticSearchConfigName { get; init; } = string.Empty;
}
