using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LFGBot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LFGBot
{
    public class Initialize
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        
        private readonly IServiceProvider _services;
        
        public Initialize()
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
            _client.Disconnected += OnDisconnect;
        
            _commands.Log += Log;
        
            _services = ConfigureServices();
        }
        
        private IServiceProvider ConfigureServices()
        {
            var map = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton<CommandHandler>()
                .AddSingleton<OldDeepService>()
                .AddSingleton<BoardService>()
                .AddSingleton(_commands);
            
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
        
        private static Task OnDisconnect(Exception exception)
        {
            Console.WriteLine("SDFKLSDLK:FSDLKFSLDKJFLKSD");
        
            return Task.CompletedTask;
        }
        
        public async Task MainAsync()
        {
            var handler = _services.GetService<CommandHandler>();
            if (handler is not null) 
                await handler.InitCommands();

            await _client.LoginAsync(
                TokenType.Bot, Environment.GetEnvironmentVariable("DiscordToken")
            );

            //Environment.GetEnvironmentVariable("TestDiscordToken"));
            
            await _client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }
    }
}