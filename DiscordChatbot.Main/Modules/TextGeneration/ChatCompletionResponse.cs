namespace DiscordChatbot.Modules.TextGeneration;

public class ChatCompletionResponse
{
    public List<Choice> choices { get; set; }
}

public class Choice
{
    public ResponseMessage message { get; set; }
}

public class ResponseMessage
{
    public string? content { get; set; }
}