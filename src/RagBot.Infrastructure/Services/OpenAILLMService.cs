using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using RagBot.Application.Interfaces;

namespace RagBot.Infrastructure.Services;

public class OpenAILLMService : ILLMService
{
    private readonly ChatClient _chatClient;
    private readonly EmbeddingClient _embeddingClient;

    public OpenAILLMService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey is missing");
        var chatModel = configuration["OpenAI:ChatModel"] ?? "gpt-4o-mini";
        var embeddingModel = configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";

        var client = new OpenAIClient(apiKey);
        _chatClient = client.GetChatClient(chatModel);
        _embeddingClient = client.GetEmbeddingClient(embeddingModel);
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var response = await _embeddingClient.GenerateEmbeddingAsync(text);
        return response.Value.ToFloats().ToArray();
    }

    public async Task<string> GetChatCompletionAsync(string prompt, string context)
    {
        var finalPrompt = $"Context information is below.\n---------------------\n{context}\n---------------------\nGiven the context information and not prior knowledge, answer the query.\nQuery: {prompt}\nAnswer:";
        
        ChatCompletion completion = await _chatClient.CompleteChatAsync(finalPrompt);
        return completion.Content[0].Text;
    }
}
