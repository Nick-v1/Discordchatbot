using Newtonsoft.Json;

namespace DiscordChatbot.Modules.TextGeneration;

public interface IConversationHistory
{
    Task<List<Conversation>> LoadConversationHistory(string fileName);
    Task SaveConversation(string userPrompt, string serviceAnswer, string userName);
    Task<string> RemoveUserConversationHistoryAsync(string fileName);
}
public class ConversationHistory : IConversationHistory
{
    private static readonly string OutputDirectory = Path.Combine($@"{AppDomain.CurrentDomain.BaseDirectory}\..\..\..", "Modules", "TextGeneration", "Outputs");
    public async Task SaveConversation(string userPrompt, string serviceAnswer, string userName)
    {
        var fileName = $"{userName}_chat.json";
        var conversationHistory = await LoadConversationHistory(fileName);

        // Add the new conversation to the history
        conversationHistory.Add(new Conversation
        {
            UserPrompt = userPrompt,
            ServiceAnswer = serviceAnswer,
            Timestamp = DateTime.UtcNow
        });
        var filepath = Path.Combine(OutputDirectory, fileName);

        Directory.CreateDirectory(OutputDirectory);
        
        // Save the updated conversation history back to the file
        await File.WriteAllTextAsync(filepath, JsonConvert.SerializeObject(conversationHistory, Formatting.Indented));
    }

    public async Task<List<Conversation>> LoadConversationHistory(string fileName)
    {
        
        var filePath = Path.Combine(OutputDirectory, fileName);
        
        if (!File.Exists(filePath))
        {
            // If the file doesn't exist, create it with an empty list
            await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(new List<Conversation>(), Formatting.Indented));
        }

        // Load the conversation history from the file
        var conversationHistoryJson = await File.ReadAllTextAsync(filePath);
        return JsonConvert.DeserializeObject<List<Conversation>>(conversationHistoryJson);
    }

    public async Task<string> RemoveUserConversationHistoryAsync(string fileName)
    {
        var filePath = Path.Combine(OutputDirectory, fileName);

        if (File.Exists(filePath))
        {
            await Task.Run(() => File.Delete(filePath));
            return "Chat has been reset!";
        }

        return "You have no saved chats!";
    }
}

public class Conversation
{
    public string UserPrompt { get; set; }
    public string ServiceAnswer { get; set; }
    public DateTime Timestamp { get; set; }
}
