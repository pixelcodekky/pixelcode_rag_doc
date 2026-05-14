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

    public async Task<string> RewriteQueryAsync(string query)
    {
        var prompt = "You are a specialized search query optimizer for a Retrieval-Augmented Generation (RAG) system. Your task is to rewrite the following user question into a highly effective, standalone search query that will be used to search a vector database containing document chunks.\n\n" +
                     "Keep these rules in mind:\n" +
                     "1. Remove conversational filler (e.g., \"Can you tell me\", \"What is\").\n" +
                     "2. Focus exclusively on the core entities, keywords, and intent.\n" +
                     "3. Return ONLY the rewritten optimization query text, and absolutely nothing else.\n\n" +
                     $"User Question: {query}";

        ChatCompletion completion = await _chatClient.CompleteChatAsync(prompt);
        return completion.Content[0].Text.Trim('"', ' ', '\n');
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

    public async IAsyncEnumerable<string> GetChatCompletionStreamAsync(string prompt, string context, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var finalPrompt = $"Context information is below.\n---------------------\n{context}\n---------------------\nGiven the context information and not prior knowledge, answer the query.\nQuery: {prompt}\nAnswer:";
        
        var options = new ChatCompletionOptions { };
        
        var chatMessages = new List<ChatMessage> { new UserChatMessage(finalPrompt) };
        var completionUpdates = _chatClient.CompleteChatStreamingAsync(chatMessages, options, cancellationToken);

        await foreach (StreamingChatCompletionUpdate update in completionUpdates)
        {
            if (update.ContentUpdate.Count > 0)
            {
                var text = update.ContentUpdate[0].Text;
                if (!string.IsNullOrEmpty(text))
                {
                    yield return text;
                }
            }
        }
    }
}
