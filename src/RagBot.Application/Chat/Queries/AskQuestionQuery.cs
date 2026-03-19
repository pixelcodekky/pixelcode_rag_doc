using MediatR;
using RagBot.Application.Interfaces;

namespace RagBot.Application.Chat.Queries;

public record AskQuestionQuery(string Question) : IRequest<string>;

public class AskQuestionHandler : IRequestHandler<AskQuestionQuery, string>
{
    private readonly IVectorStore _vectorStore;
    private readonly ILLMService _llmService;

    public AskQuestionHandler(IVectorStore vectorStore, ILLMService llmService)
    {
        _vectorStore = vectorStore;
        _llmService = llmService;
    }

    public async Task<string> Handle(AskQuestionQuery request, CancellationToken cancellationToken)
    {
        // 1. Generate embedding for the question
        var questionEmbedding = await _llmService.GenerateEmbeddingAsync(request.Question);

        // 2. Search for similar chunks
        var similarChunks = await _vectorStore.SearchSimilarChunksAsync(questionEmbedding);

        // 3. Construct context
        var context = string.Join("\n\n", similarChunks.Select(c => c.Content));

        // 4. Get chat completion
        var answer = await _llmService.GetChatCompletionAsync(request.Question, context);

        return answer;
    }
}
