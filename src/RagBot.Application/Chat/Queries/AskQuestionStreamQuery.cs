using System.Runtime.CompilerServices;
using MediatR;
using RagBot.Application.Interfaces;

namespace RagBot.Application.Chat.Queries;

public record AskQuestionStreamQuery(string Question) : IStreamRequest<string>;

public class AskQuestionStreamHandler : IStreamRequestHandler<AskQuestionStreamQuery, string>
{
    private readonly IVectorStore _vectorStore;
    private readonly ILLMService _llmService;

    public AskQuestionStreamHandler(IVectorStore vectorStore, ILLMService llmService)
    {
        _vectorStore = vectorStore;
        _llmService = llmService;
    }

    public async IAsyncEnumerable<string> Handle(AskQuestionStreamQuery request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // 1. Advanced RAG Tool: Rewrite user query for better search retrieval
        var optimizedQuery = await _llmService.RewriteQueryAsync(request.Question);

        // 2. Generate embedding for the OPTIMIZED query
        var questionEmbedding = await _llmService.GenerateEmbeddingAsync(optimizedQuery);

        // 3. Search for similar chunks using the optimized embedding
        var similarChunks = await _vectorStore.SearchSimilarChunksAsync(questionEmbedding);

        // 4. Construct context
        var context = string.Join("\n\n", similarChunks.Select(c => c.Content));

        // 5. Stream chat completion
        var stream = _llmService.GetChatCompletionStreamAsync(request.Question, context, cancellationToken);

        await foreach (var chunk in stream.WithCancellation(cancellationToken))
        {
            yield return chunk;
        }
    }
}
