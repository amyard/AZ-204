using System.Collections.Concurrent;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Options;
using SemanticSearch.API.Interfaces;
using SemanticSearch.API.Models;
using SemanticSearch.API.Providers;

namespace SemanticSearch.API.Factory;

public class SemanticSearchServiceFactory()
{
    private ConcurrentDictionary<string, ISemanticSearchService> _services = new();
    private readonly AzureConfigs _azureConfigs;

    public SemanticSearchServiceFactory(IOptions<AzureConfigs> azureConfigs) : this()
    {
        _azureConfigs = azureConfigs.Value;
    }

    public ISemanticSearchService GetSemanticSearchService(string indexName = null)
    {
        indexName ??= _azureConfigs.SemanticSearchIndexNameProdEnv;

        return _services.GetOrAdd(indexName, (indexNameR) =>
            new SemanticSearchService(GetSemanticSearchIndexClient(), GetSemanticSearchClient(indexNameR), _azureConfigs));
    }
    
    # region private methods
    // Create a SearchIndexClient to send create/delete index commands
    private SearchIndexClient GetSemanticSearchIndexClient()
    {
        string endpoint = _azureConfigs.SemanticSearchEndpoint;
        string key = _azureConfigs.SemanticSearchAPIKey;
        SearchIndexClient adminClient = new (new Uri(endpoint), new AzureKeyCredential(key));

        return adminClient;
    }

    // Create a SearchClient to load and query documents
    private SearchClient GetSemanticSearchClient(string indexName)
    {
        string endpoint = _azureConfigs.SemanticSearchEndpoint;
        string key = _azureConfigs.SemanticSearchAPIKey;
        SearchClient searchClient = new (new Uri(endpoint), indexName, new AzureKeyCredential(key));

        return searchClient;
    }
    # endregion
}
