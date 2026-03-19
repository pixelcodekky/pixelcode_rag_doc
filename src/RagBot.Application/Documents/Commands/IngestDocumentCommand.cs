using MediatR;
using Pgvector;
using RagBot.Application.Interfaces;
using RagBot.Domain.Entities;

namespace RagBot.Application.Documents.Commands;

public record IngestDocumentCommand(string Content, string Metadata) : IRequest<List<Guid>>;

public class IngestDocumentHandler : IRequestHandler<IngestDocumentCommand, List<Guid>>
{
    private readonly IVectorStore _vectorStore;
    private readonly ILLMService _llmService;

    public IngestDocumentHandler(IVectorStore vectorStore, ILLMService llmService)
    {
        _vectorStore = vectorStore;
        _llmService = llmService;
    }

    public async Task<List<Guid>> Handle(IngestDocumentCommand request, CancellationToken cancellationToken)
    {
        var textChunks = ChunkText(request.Content, 1200, 200);
        var chunkIds = new List<Guid>();

        foreach (var textChunk in textChunks)
        {
            var embedding = await _llmService.GenerateEmbeddingAsync(textChunk);
            
            var chunk = new DocumentChunk
            {
                Content = textChunk,
                SourceMetadata = request.Metadata,
                Embedding = new Vector(embedding)
            };

            await _vectorStore.AddChunkAsync(chunk);
            chunkIds.Add(chunk.Id);
        }
        
        return chunkIds;
    }

    private List<string> ChunkText(string text, int chunkSize, int overlap)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<string>();

        var chunks = new List<string>();
        int position = 0;

        while (position < text.Length)
        {
            int length = Math.Min(chunkSize, text.Length - position);
            chunks.Add(text.Substring(position, length));
            
            position += chunkSize - overlap;
        }

        return chunks;
    }
}
