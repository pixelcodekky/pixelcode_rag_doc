using RagBot.Domain.Entities;

namespace RagBot.Application.Interfaces;

public interface IVectorStore
{
    Task AddChunkAsync(DocumentChunk chunk);
    Task<List<DocumentChunk>> SearchSimilarChunksAsync(float[] embedding, int limit = 5);
}
