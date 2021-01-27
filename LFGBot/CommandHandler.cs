using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace LFGBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(IServiceProvider services, CommandService commands, DiscordSocketClient client)
        {
            _commands = commands;
            _services = services;
            _client = client;
        }
        
        public async Task InitCommands()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage msg)) return;

            if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) return;
        
            var pos = 0;
            if (msg.HasCharPrefix('=', ref pos) /* || msg.HasMentionPrefix(_client.CurrentUser, ref pos) */)
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
}