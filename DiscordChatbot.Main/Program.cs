using System.Reflection;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordChatbot.Modules.TextGeneration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    private static IConfigurationRoot configuration;
    private DiscordSocketConfig _config;
    private IServiceCollection _collection;
    private IServiceProvider _services;
    private CommandService _commands;
    private DiscordSocketClient _client;
    
    public static async Task Main(string[] args)
    {
        await new Program().MainAsync();
    }
    
    public async Task MainAsync()
    {
        configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _config = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.All & ~GatewayIntents.GuildScheduledEvents & ~GatewayIntents.GuildPresences &
                             ~GatewayIntents.GuildInvites
        };

        _collection = new ServiceCollection()
            .AddSingleton(_config)
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<DiscordSocketClient>()
            .AddScoped<CommandService>()
            .AddSingleton<ITextGenerationModule, TextGenerationModule>()
            .AddScoped<IConversationHistory, ConversationHistory>();
        
        _services = _collection.BuildServiceProvider();

        _commands = _services.GetRequiredService<CommandService>();

        _client = _services.GetRequiredService<DiscordSocketClient>();
        
        string token = configuration.GetSection("DISCORD_TOKEN").Value;

        await InstallCommandsAsync();

        await _client.LoginAsync(TokenType.Bot, token); //GetEnvironmentVariable("token");
        await _client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }

     private async Task InstallCommandsAsync()
     {
         _client.MessageReceived += HandleCommandAsync;
         _client.Log += LogMessage;
         await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
     }
     
     private Task LogMessage(LogMessage msg)
     {
         Console.WriteLine(msg.ToString());
         return Task.CompletedTask;
     }

     private async Task HandleCommandAsync(SocketMessage messageParam)
     {
         try
         {
             var message = messageParam as SocketUserMessage;
             if (message == null) return;

             var argPos = 0;
             if (!message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.Author.IsBot) return;

             var contentAfterMention = message.Content.Substring(argPos).Trim();
             var context = new SocketCommandContext(_client, message);
             
             var username = message.Author.Username;
             var filePath = $"{username}_chat.json";
             var conversationHistory = _services.GetRequiredService<IConversationHistory>();

             if (contentAfterMention.StartsWith("!reset"))
             {
                 var reply = await conversationHistory.RemoveUserConversationHistoryAsync(filePath);
                 await message.ReplyAsync(reply);
                 return;
             }

             var textGenerationModule = _services.GetRequiredService<ITextGenerationModule>();
             
             var history = await conversationHistory.LoadConversationHistory(filePath);

             var conversation = BuildConversation(history, contentAfterMention);

             var response = await textGenerationModule.AskQuestion(conversation);
             await conversationHistory.SaveConversation(contentAfterMention, response, username);
             
             Console.WriteLine(conversation);
             
             await message.ReplyAsync(response);
             return;
         }
         catch (Exception e)
         {
             await messageParam.Channel.SendMessageAsync(e.Message);
             return;
         }
     }

     private string BuildConversation(List<Conversation> history, string userPrompt)
     {
         var conversation = new StringBuilder();
         var character = configuration.GetSection("Character_Name").Value;

         foreach (var fact in history)
         {
             conversation.AppendLine($"User: {fact.UserPrompt}");
             conversation.AppendLine($"{character}: {fact.ServiceAnswer}");
         }

         conversation.AppendLine($"User: {userPrompt}");

         return conversation.ToString();
     }
}