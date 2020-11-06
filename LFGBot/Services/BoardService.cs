using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace LFGBot.Services
{
    public class BoardService
    {
        public ISocketMessageChannel Channel;

        private DiscordSocketClient _client;
        
        public BoardService(DiscordSocketClient client)
        {
            _client = client;
            
            _client.ReactionAdded += OnReactionAdded;
            _client.ReactionRemoved += OnReactionRemoved;
        }
        
        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            throw new NotImplementedException();
        }

        
    }
}