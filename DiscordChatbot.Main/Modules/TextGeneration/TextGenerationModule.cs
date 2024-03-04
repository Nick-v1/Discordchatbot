using System.Text;
using Microsoft.Extensions.Configuration;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DiscordChatbot.Modules.TextGeneration;

public interface ITextGenerationModule
{
    Task<string?> AskQuestion(string prompt);
}

public class TextGenerationModule : ITextGenerationModule
{
    private readonly ChatCompletionModel _chatCompletionModel = new();
    private readonly IConfiguration _configuration;

    public TextGenerationModule(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string?> AskQuestion(string prompt)
    {
        _chatCompletionModel.Character = _configuration.GetSection("Character_Name").Value;
        _chatCompletionModel.Greeting = "Hello!";
        _chatCompletionModel.InstructionTemplate = "Alpaca";
        _chatCompletionModel.MaxTokens = 300;
        _chatCompletionModel.Mode = "chat";
        _chatCompletionModel.Temperature = 0.7; //Higher than 0.7 diverse and creative. Lower than 0.5 deterministic and focused.
        _chatCompletionModel.TopK = 100;         //The higher the value the more complex and creative words.
        _chatCompletionModel.TopP = 0.6;       //0.5 - 0.7 balances variety and focus. lower than 0.7 less engaging or informative responses. Higher than 0.9 vastly broader range of words, leading to potentially interesting but potentially irrelevant or nonsensical outputs.
        _chatCompletionModel.TypicalP = 1;
        _chatCompletionModel.Messages = new List<Message>
        {
            new()
            {
                Content = prompt,
                Role = "user"
            }
        };

        var jsonString = JsonSerializer.Serialize(_chatCompletionModel);
        Console.WriteLine(jsonString);

        using var client = new HttpClient();
        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

        try
        {
            using var response = await client.PostAsync("http://127.0.0.1:5000/v1/chat/completions", content);
        
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
            
                var responseObject = JsonSerializer.Deserialize<ChatCompletionResponse>(responseString);

                var reply = responseObject.choices[0].message.content;

                Console.WriteLine(reply);
                return reply;
            }
            
            Console.WriteLine($"Error: {response.StatusCode}, {response.RequestMessage}");
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occured: {e.Message}");
            if (e.HResult.Equals(-2147467259))
            {
                return null;
            }
            return null;
        }
        
    }
}