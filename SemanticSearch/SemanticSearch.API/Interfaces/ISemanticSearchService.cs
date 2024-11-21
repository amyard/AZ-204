using Azure;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using SemanticSearch.API.Models;

namespace SemanticSearch.API.Interfaces;

public interface ISemanticSearchService
{
    // GENERAL
    Task<Response<SearchIndex>> CreateSearchIndex(string indexName = null);

    // CRUD
    Task<Response<IndexDocumentsResult>> IndexDocument(DocumentSearchIndex documentSearchIndex);
    Task<Response> DeleteDocuments(List<DocumentSearchIndex> indexDocuments);
    Task<Response> DeleteDocument(DocumentSearchIndex documentSearchIndex);
    Task<IndexingResult> UpsertDocument(DocumentSearchIndex documentSearchIndex);
    

    // SEARCHING
    Task<List<DocumentSearchIndex>> ExecuteLiteralQuery(string userId, string searchQuery);
    Task<List<DocumentSearchIndex>> ExecuteSemanticQuery(string userId, string searchQuery);
}
