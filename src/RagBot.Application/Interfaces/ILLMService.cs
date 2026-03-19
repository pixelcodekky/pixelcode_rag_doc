namespace RagBot.Application.Interfaces;

public interface ILLMService
{
    Task<float[]> GenerateEmbeddingAsync(string text);
    Task<string> GetChatCompletionAsync(string prompt, string context);
}
