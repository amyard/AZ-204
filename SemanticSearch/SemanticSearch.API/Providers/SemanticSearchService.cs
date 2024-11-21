using System.Globalization;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using SemanticSearch.API.Interfaces;
using SemanticSearch.API.Models;

namespace SemanticSearch.API.Providers;

public class SemanticSearchService: ISemanticSearchService
{
    // a SearchIndexClient to send create/delete index commands
    private readonly SearchIndexClient _adminClient;

    // a SearchClient to load and query documents
    private readonly SearchClient _searchClient;

    private readonly AzureConfigs _azureConfigs;

    public SemanticSearchService(SearchIndexClient adminClient, SearchClient searchClient, AzureConfigs azureConfigs)
    {
        _azureConfigs = azureConfigs;
        _adminClient = adminClient ?? throw new ArgumentNullException(nameof(adminClient));
        _searchClient = searchClient ?? throw new ArgumentNullException(nameof(searchClient));
    }
    
    # region GENERAL
    public async Task<Response<SearchIndex>> CreateSearchIndex(string indexName = null)
    {
        indexName ??= _azureConfigs.SemanticSearchIndexNameProdEnv;
        IList<SearchField> searchFields = new FieldBuilder().Build(typeof(DocumentSearchIndex));
        SearchIndex definition = new (indexName, searchFields);

        definition.SemanticSearch = new Azure.Search.Documents.Indexes.Models.SemanticSearch
        {
            Configurations =
            {
                new SemanticConfiguration(_azureConfigs.SemanticSearchConfigName, new SemanticPrioritizedFields()
                {
                    TitleField = new SemanticField(nameof(DocumentSearchIndex.Title)),
                    ContentFields =
                    {
                        new SemanticField(nameof(DocumentSearchIndex.Content))
                    },
                    KeywordsFields =
                    {
                        new SemanticField(nameof(DocumentSearchIndex.Id)),
                        new SemanticField(nameof(DocumentSearchIndex.UserId))
                    }
                })
            }
        };

        Response<SearchIndex> response = await _adminClient.CreateOrUpdateIndexAsync(definition).ConfigureAwait(false);
        return response;
    }
    # endregion

    # region CRUD
    public async Task<Response<IndexDocumentsResult>> IndexDocument(DocumentSearchIndex documentSearchIndex)
    {
        IndexDocumentsBatch<DocumentSearchIndex> batch = IndexDocumentsBatch.Upload([documentSearchIndex]);
        Response<IndexDocumentsResult> response = await _searchClient.IndexDocumentsAsync(batch).ConfigureAwait(false);
        return response;
    }

    public async Task<Response> DeleteDocuments(List<DocumentSearchIndex> indexDocuments)
    {
        try
        {
            Response<IndexDocumentsResult> response = await _searchClient.DeleteDocumentsAsync(indexDocuments).ConfigureAwait(false);
            return response.GetRawResponse();
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public async Task<Response> DeleteDocument(DocumentSearchIndex documentSearchIndex) => await DeleteDocuments([documentSearchIndex]).ConfigureAwait(false);

    public async Task<IndexingResult> UpsertDocument(DocumentSearchIndex documentSearchIndex)
    {
        DocumentSearchIndex document = new ();

        try
        {
            // retrieve document
            Response<DocumentSearchIndex> response = await _searchClient.GetDocumentAsync<DocumentSearchIndex>(documentSearchIndex.UserId);
            document = response.Value;

            // Update fields
            document.Title = documentSearchIndex.Title;
            document.Content = documentSearchIndex.Content;
            document.Modified = documentSearchIndex.Modified;
        }
        // Service request failed. Status: 404 (Not Found)
        catch (RequestFailedException e)
        {
            // document doesn't exist. need to create a new index
            document = documentSearchIndex;
        }

        // Upsert the updated document back into the index
        List<DocumentSearchIndex> updatedDocument = new() { document };

        // Upload the documents to the index
        try
        {
            IndexDocumentsResult result = await _searchClient.MergeOrUploadDocumentsAsync(updatedDocument).ConfigureAwait(false);
            return result.Results[0];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while updating the document: {ex.Message}");
            return null;
        }
    }
    # endregion

    # region SEARCHING
    public async Task<List<DocumentSearchIndex>> ExecuteLiteralQuery(string userId, string searchQuery)
    {
        SearchOptions searchOptions = GetLiteralSearchOptions(userId);
        SearchResults<DocumentSearchIndex> response = await _searchClient.SearchAsync<DocumentSearchIndex>(searchQuery, searchOptions).ConfigureAwait(false);
        List<DocumentSearchIndex> result = response.GetResults().Select(x => x.Document).ToList();

        return result;
    }

    public async Task<List<DocumentSearchIndex>> ExecuteSemanticQuery(string userId, string searchQuery)
    {
        SearchOptions searchOptions = GetSemanticSearchOptions(userId);
        SearchResults<DocumentSearchIndex> response = await _searchClient.SearchAsync<DocumentSearchIndex>(searchQuery, searchOptions).ConfigureAwait(false);
        List<DocumentSearchIndex> result = response.GetResults().Select(x => x.Document).ToList();

        return result;
    }
    # endregion
    
    # region PRIVATE methods
    private SearchOptions GetSemanticSearchOptions(string userId)
    {
        // 'orderBy' is not supported when 'queryType' is set to 'semantic'.
        SearchOptions searchOptions = GetGeneralSearchOptions(userId);
        searchOptions.QueryType = SearchQueryType.Semantic;
        searchOptions.SemanticSearch = new()
        {
            SemanticConfigurationName = _azureConfigs.SemanticSearchConfigName,
            QueryCaption = new(QueryCaptionType.Extractive)
        };

        return searchOptions;
    }

    private SearchOptions GetLiteralSearchOptions(string userId)
    {
        SearchOptions searchOptions = GetGeneralSearchOptions(userId);
        searchOptions.OrderBy.Add($"{nameof(DocumentSearchIndex.Modified)} asc");

        return searchOptions;
    }

    private SearchOptions GetGeneralSearchOptions(string userId)
    {
        string thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

        SearchOptions searchOptions = new()
        {
            Filter = $"{nameof(DocumentSearchIndex.UserId)} eq '{userId}' and {nameof(DocumentSearchIndex.Modified)} ge {thirtyDaysAgo}",
            Select =
            {
                nameof(DocumentSearchIndex.Title),
                nameof(DocumentSearchIndex.Content),
                nameof(DocumentSearchIndex.Modified),
            },
        };

        return searchOptions;
    }
    # endregion
}
