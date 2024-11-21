using Azure.Search.Documents.Indexes;

namespace SemanticSearch.Console.Models;

public class IndexDocument(string id, string userId, string title, string content)
{
    public IndexDocument(string id, string userId, string title, string content, DateTimeOffset modified): this(id, userId, title, content)
    {
        Modified = modified;
    }

    [SimpleField]
    public string Id { get; set; }
    
    [SimpleField(IsFilterable = true)]
    public string UserId { get; set; }

    [SearchableField(IsFilterable = true, IsSortable = true)]
    public string Title { get; set; }
    
    // [SearchableField(IsFilterable = true, IsSortable = true, AnalyzerName = LexicalAnalyzerName.Values.FrLucene)] - LexicalAnalyzerName.Values.FrLucene - use for different languages
    // Do not add any attributes like IsFilterable = true, IsSortable = true . It will cause an error for huge document
    // Field 'Content' contains a term that is too large to process. The max length for UTF-8 encoded terms is 32766 bytes. The most likely cause of this error is that filtering, sorting, and/or faceting are enabled on this field, which causes the entire field value to be indexed as a single term. Please avoid the use of these options for large fields.
    [SearchableField]
    public string Content { get; set; }
    
    [SimpleField(IsFilterable = true, IsSortable = true)]
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    [SimpleField(IsFilterable = true, IsSortable = true)]
    public DateTimeOffset Modified { get; set; } = DateTimeOffset.UtcNow;
}
