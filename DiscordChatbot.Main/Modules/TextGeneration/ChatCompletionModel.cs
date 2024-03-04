using System.Text.Json.Serialization;

namespace DiscordChatbot.Modules.TextGeneration;

public class ChatCompletionModel
{
    [JsonPropertyName("character")]
    public string Character { get; set; }
    
    [JsonPropertyName("greeting")]
    public string Greeting { get; set; }
    
    [JsonPropertyName("instruction_template")]
    public string InstructionTemplate { get; set; }
    
    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; }
    
    [JsonPropertyName("messages")]
    public List<Message> Messages { get; set; }
    
    [JsonPropertyName("mode")]
    public string Mode { get; set; }
    
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }
    
    [JsonPropertyName("top_k")]
    public int TopK { get; set; }
    
    [JsonPropertyName("top_p")]
    public double TopP { get; set; }
    
    [JsonPropertyName("typical_p")]
    public double TypicalP { get; set; }
}

public class Message
{
    [JsonPropertyName("content")]
    public string Content { get; set; }
    
    [JsonPropertyName("role")]
    public string Role { get; set; }
}