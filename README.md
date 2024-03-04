The setup requires <a href="https://github.com/oobabooga/text-generation-webui"> Text Generation Webui</a>, with your
preferred model, make sure you enable the `--api` flag in `CMD_FLAGS.txt.` of the <span style="color:aqua">Text
Generation Webui</span> project.

Create a `appsettings.json` file inside `DiscordChatbot.Main` project, with the following structure:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "DISCORD_TOKEN": "your discord bot's API key",
  "Character_Name": "your character's name in text generation webui"
}
```

Run the Project: cd inside `DiscordChatbot.Main` and run `dotnet run`. _(Requires .Net 8.0)_