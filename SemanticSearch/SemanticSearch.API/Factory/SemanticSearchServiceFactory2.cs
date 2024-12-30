using System.Collections.Concurrent;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Options;
using SemanticSearch.API.Interfaces;
using SemanticSearch.API.Models;
using SemanticSearch.API.Providers;

namespace SemanticSearch.API.Factory
{
    public static class SemanticSearchServiceFactory2
    {
        private static readonly ConcurrentDictionary<string, ISemanticSearchService> _services = new();
        private static AzureConfigs _azureConfigs;

        // Static constructor for initializing static fields
        static SemanticSearchServiceFactory2() { }

        // Method to configure the factory with Azure configurations
        public static void Configure(IOptions<AzureConfigs> azureConfigs)
        {
            _azureConfigs = azureConfigs.Value;
        }

        public static ISemanticSearchService GetSemanticSearchService(string indexName = null)
        {
            if (_azureConfigs == null)
            {
                throw new InvalidOperationException("SemanticSearchServiceFactory is not configured. Call Configure() first.");
            }

            indexName ??= _azureConfigs.SemanticSearchIndexNameProdEnv;

            return _services.GetOrAdd(indexName, indexNameR =>
                new SemanticSearchService(GetSemanticSearchIndexClient(), GetSemanticSearchClient(indexNameR), _azureConfigs));
        }

        #region private methods
        // Create a SearchIndexClient to send create/delete index commands
        private static SearchIndexClient GetSemanticSearchIndexClient()
        {
            string endpoint = _azureConfigs.SemanticSearchEndpoint;
            string key = _azureConfigs.SemanticSearchAPIKey;
            return new SearchIndexClient(new Uri(endpoint), new AzureKeyCredential(key));
        }

        // Create a SearchClient to load and query documents
        private static SearchClient GetSemanticSearchClient(string indexName)
        {
            string endpoint = _azureConfigs.SemanticSearchEndpoint;
            string key = _azureConfigs.SemanticSearchAPIKey;
            return new SearchClient(new Uri(endpoint), indexName, new AzureKeyCredential(key));
        }
        #endregion
    }
}
