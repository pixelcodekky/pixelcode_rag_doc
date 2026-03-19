using Pgvector;

namespace RagBot.Domain.Entities;

public class DocumentChunk
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; } = string.Empty;
    public string SourceMetadata { get; set; } = string.Empty;
    public Vector? Embedding { get; set; }
}
