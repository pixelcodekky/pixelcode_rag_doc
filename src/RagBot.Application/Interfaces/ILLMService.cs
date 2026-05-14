namespace RagBot.Application.Interfaces;

public interface ILLMService
{
    Task<string> RewriteQueryAsync(string query);
    Task<float[]> GenerateEmbeddingAsync(string text);
    Task<string> GetChatCompletionAsync(string prompt, string context);
    IAsyncEnumerable<string> GetChatCompletionStreamAsync(string prompt, string context, CancellationToken cancellationToken = default);
}
