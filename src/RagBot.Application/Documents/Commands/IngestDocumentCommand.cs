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
        // Try to split by paragraphs first
        var paragraphs = text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        var currentChunk = new System.Text.StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            if (currentChunk.Length + paragraph.Length > chunkSize && currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
                
                // Add overlap: keep the trailing end of the previous chunk
                var lastContent = currentChunk.ToString();
                currentChunk.Clear();
                
                if (overlap > 0 && lastContent.Length > overlap)
                {
                    var overlapString = lastContent.Substring(lastContent.Length - overlap);
                    // Avoid cutting words in half if possible
                    var firstSpace = overlapString.IndexOf(' ');
                    if (firstSpace >= 0 && firstSpace < overlapString.Length - 1)
                        overlapString = overlapString.Substring(firstSpace + 1);
                    currentChunk.Append(overlapString).Append(" ");
                }
            }

            // If a single paragraph is too large, split by sentences
            if (paragraph.Length > chunkSize)
            {
                var sentences = paragraph.Split(new[] { ". ", "? ", "! ", ".\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var sentence in sentences)
                {
                    // Add back some punctuation as a best guess
                    var sentenceWithPunctuation = sentence + ". ";
                    if (currentChunk.Length + sentenceWithPunctuation.Length > chunkSize && currentChunk.Length > 0)
                    {
                        chunks.Add(currentChunk.ToString().Trim());
                        currentChunk.Clear();
                    }
                    
                    if (sentenceWithPunctuation.Length > chunkSize)
                    {
                        // Final fallback for giant strings without punctuation
                        for (int i = 0; i < sentenceWithPunctuation.Length; i += chunkSize)
                            chunks.Add(sentenceWithPunctuation.Substring(i, Math.Min(chunkSize, sentenceWithPunctuation.Length - i)));
                    }
                    else
                    {
                        currentChunk.Append(sentenceWithPunctuation);
                    }
                }
            }
            else
            {
                // Paragraph fits in chunk size
                currentChunk.Append(paragraph).Append("\n\n");
            }
        }

        if (currentChunk.Length > 0)
        {
            var finalChunk = currentChunk.ToString().Trim();
            if (!string.IsNullOrEmpty(finalChunk))
                chunks.Add(finalChunk);
        }

        return chunks;
    }
}
