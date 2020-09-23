using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LFGBot.Modules;

class Program
{
    static void Main(string[] args)
    {
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    private readonly DiscordSocketClient _client;
    
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;

    private Program()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Info,
            
            // If you or another service needs to do anything with messages
            // (eg. checking Reactions, checking the content of edited/deleted messages),
            // you must set the MessageCacheSize. You may adjust the number as needed.
            //MessageCacheSize = 50,

            // If your platform doesn't have native WebSockets,
            // add Discord.Net.Providers.WS4Net from NuGet,
            // add the `using` at the top, and uncomment this line:
            //WebSocketProvider = WS4NetProvider.Instance
        });
        
        _commands = new CommandService(new CommandServiceConfig
        {
            LogLevel = LogSeverity.Info,
            
            CaseSensitiveCommands = false,
        });
        
        _client.Log += Log;
        _commands.Log += Log;
        
        _services = ConfigureServices();
        
    }
    private static IServiceProvider ConfigureServices()
    {
        var map = new ServiceCollection(); 
            //.AddSingleton(new SomeServiceClass());
            
        return map.BuildServiceProvider();
    }
    
    private static Task Log(LogMessage message)
    {
        switch (message.Severity)
        {
            case LogSeverity.Critical:
            case LogSeverity.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case LogSeverity.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogSeverity.Info:
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case LogSeverity.Verbose:
            case LogSeverity.Debug:
                Console.ForegroundColor = ConsoleColor.DarkGray;
                break;
        }
        Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}");
        Console.ResetColor();
        
        return Task.CompletedTask;
    }

    private async Task MainAsync()
    {
        await InitCommands();

        await _client.LoginAsync(TokenType.Bot,
            Environment.GetEnvironmentVariable("DiscordToken"));
        await _client.StartAsync();

        await Task.Delay(Timeout.Infinite);
    }

    private async Task InitCommands()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        //await _commands.AddModuleAsync<InfoModule>(_services);

        _client.MessageReceived += HandleCommandAsync;
    }

    private async Task HandleCommandAsync(SocketMessage arg)
    {
        var msg = arg as SocketUserMessage;
        if (msg == null) return;

        if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) return;
        
        int pos = 0;
        if (msg.HasCharPrefix('!', ref pos) /* || msg.HasMentionPrefix(_client.CurrentUser, ref pos) */)
        {
            var context = new SocketCommandContext(_client, msg);
            
            var result = await _commands.ExecuteAsync(context, pos, _services);

            // Uncomment the following lines if you want the bot
            // to send a message if it failed.
            // This does not catch errors from commands with 'RunMode.Async',
            // subscribe a handler for '_commands.CommandExecuted' to see those.
            //if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            //    await msg.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
