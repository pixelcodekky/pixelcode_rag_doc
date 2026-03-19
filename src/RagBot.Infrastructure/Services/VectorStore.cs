using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using RagBot.Application.Interfaces;
using RagBot.Domain.Entities;
using RagBot.Infrastructure.Persistence;

namespace RagBot.Infrastructure.Services;

public class VectorStore : IVectorStore
{
    private readonly AppDbContext _context;

    public VectorStore(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddChunkAsync(DocumentChunk chunk)
    {
        _context.DocumentChunks.Add(chunk);
        await _context.SaveChangesAsync();
    }

    public async Task<List<DocumentChunk>> SearchSimilarChunksAsync(float[] embedding, int limit = 5)
    {
        var vector = new Vector(embedding);
        
        // Using Cosine Similarity with pgvector
        return await _context.DocumentChunks
            .OrderBy(c => c.Embedding!.CosineDistance(vector))
            .Take(limit)
            .ToListAsync();
    }
}
