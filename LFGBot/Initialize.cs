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

        private Config _config;
        
        public Initialize(Config config)
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
            
                // If you or another service needs to do anything with messages
                // (eg. checking Reactions, checking the content of edited/deleted messages),
                // you must set the MessageCacheSize. You may adjust the number as needed.
                MessageCacheSize = 50,

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

            _config = config;
            
            _services = ConfigureServices();
        }
        
        private IServiceProvider ConfigureServices()
        {
            var map = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_config)
                .AddSingleton<StatsService>()
                .AddSingleton<DeepService>()
                .AddSingleton<BoardService>()
                .AddSingleton<CommandHandler>();
            
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
                default:
                    throw new ArgumentOutOfRangeException();
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
                TokenType.Bot, _config.CurrentToken
            );
            
            await _client.StartAsync();

            await Task.Run(ProcessCommands);
        }

        private void ProcessCommands()
        {
            var keepRunning = true;
            while (keepRunning)
            {
                var command = Console.ReadLine();

                switch (command?.ToLower())
                {
                    case "stop" or "s":
                        _services.GetService<DeepService>()?.StopService();
                        keepRunning = false;
                        break;
                    case "change" or "c":
                        _services.GetService<DeepService>()?.QueueMessage();
                        break;
                }
            }
        }
    }
}