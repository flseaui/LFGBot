using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace LFGBot.Services
{
    public class BoardService
    {
        public record Board
        {
            // emote : reactRequirement
            public Dictionary<IEmote, uint> Triggers { get; init; }
            public ISocketMessageChannel Channel;
        }
        
        private readonly DiscordSocketClient _client;
        private readonly Random _random = new();

        // channelID : messages
        private readonly Dictionary<ulong, List<IMessage>> _cachedBoards = new();
        
        // channelID : messageIDs
        private readonly Dictionary<ulong, HashSet<ulong>> _chosenMessages = new();

        private readonly Dictionary<IEmote, Board> _boards = new();

        public BoardService(DiscordSocketClient client)
        {
            _client = client;
            /*_client.ReactionAdded += OnReactionAdded;
            _client.ReactionRemoved += OnReactionRemoved;
            _client.Ready += OnClientReady;
            _client.MessageReceived += OnMessageReceived;
            _client.MessageDeleted += OnMessageDeleted;*/
        }

        public void CreateBoard(IEmote emote, uint reactReq, ISocketMessageChannel channel)
        {
            var board = new Board
            {
                Triggers = new Dictionary<IEmote, uint>
                {
                    [emote] = reactReq
                },
                Channel = channel
            };
            _boards.Add(emote, board);
        }
        
        // TODO def not best method, gets exponentially slower
        public IMessage GetUniqueRandomMessage(ulong boardChannelId)
        {
            if (!_cachedBoards.ContainsKey(boardChannelId))
            {
                Console.Error.WriteLine($"Message was requested from channel with ID {boardChannelId} but it is not a cached board channel!");
                return null;
            }
            
            var board = _cachedBoards[boardChannelId];
            var used = _chosenMessages[boardChannelId];
            
            int i;
            do
            {
                // restart once all messages have been chosen
                if (used.Count == board.Count)
                    _chosenMessages[boardChannelId].Clear();
                
                i = _random.Next(0, board.Count);
            } 
            while (used.Contains(board[i].Id));

            return board[i];
        }

        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (message.HasValue && _boards.ContainsKey(reaction.Emote))
            {
                var msg = message.Value;
                if (msg.Reactions[reaction.Emote].ReactionCount < _boards[reaction.Emote].Triggers[reaction.Emote])
                {
                    
                }
            }
        }

        private async Task OnReactionAdded(
            Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction
        )
        {
            if (message.HasValue && _boards.ContainsKey(reaction.Emote))
            {
                var msg = message.Value;
                if (msg.Reactions[reaction.Emote].ReactionCount >= _boards[reaction.Emote].Triggers[reaction.Emote])
                {
                    var builder = new EmbedBuilder();

                    var guild = ((SocketGuildChannel) channel).Guild;
                    var user = guild.GetUser(msg.Author.Id);

                    var avatarUrl = msg.Author.GetAvatarUrl(ImageFormat.Png);
                    var name = user != null ? user.Nickname :
                        msg.Author != null ? msg.Author.Username : "invalid-user";

                    builder.WithAuthor(name, avatarUrl);

                    builder.WithDescription(msg.Content);
                    builder.WithFooter($"{message.Id} • {msg.Timestamp.Date.ToShortDateString()}");

                    if (msg.Attachments.Any())
                    {
                        var attachment = msg.Attachments.First();
                        if (attachment.Filename.EndsWith(".png") || attachment.Filename.EndsWith(".jpg"))
                            builder.WithImageUrl(attachment.Url);
                        else
                            builder.WithUrl(attachment.Url);
                    }

                    builder.WithColor(Color.Purple);
                    var embed = builder.Build();

                    await _boards[reaction.Emote].Channel.SendMessageAsync("", false, embed);
                }
            }
        }

        private Task OnMessageDeleted(Cacheable<IMessage, ulong> msg, ISocketMessageChannel channel)
        {
            if (_cachedBoards.ContainsKey(channel.Id))
            {
                Console.Write($"Message {msg.Id} ");
                if (msg.Value.Embeds.Any())
                    Console.Write($"with {msg.Value.Embeds.Count} embeds ");
                if (msg.Value.Attachments.Any())
                    Console.Write($"and {msg.Value.Attachments.Count} attachments ");
                Console.WriteLine($"from board {channel.Name} | {channel.Id} has been deleted.");
                Console.WriteLine("Message contained:");
                Console.WriteLine(msg.Value.Content);
                
                _cachedBoards[channel.Id].Remove(msg.Value);
            }

            return Task.CompletedTask;
        }

        private Task OnMessageReceived(SocketMessage msg)
        {
            if (_cachedBoards.ContainsKey(msg.Channel.Id))
                _cachedBoards[msg.Channel.Id].Add(msg);

            return Task.CompletedTask;
        }
        
        private Task OnClientReady()
        {
            // lfg - stars and quotes
            _cachedBoards.Add(729759705190367273, CacheChannelMessages(729759705190367273));
            _chosenMessages[729759705190367273] = new HashSet<ulong>();
            
            return Task.CompletedTask;
        }
        
        private List<IMessage> CacheChannelMessages(ulong channelId) =>
            CacheChannelMessages(_client.GetChannel(channelId) as IMessageChannel);
        
        private List<IMessage> CacheChannelMessages(IMessageChannel channel)
        {
            var allMessages = new List<IMessage>();
            var lastMessage = channel.GetMessagesAsync(1).Flatten().ToListAsync().Result[0];
            allMessages.Add(lastMessage);
            while (true)
            { 
                var messages = channel.GetMessagesAsync(lastMessage, Direction.Before).Flatten().ToListAsync().Result;
                Console.WriteLine($"got {messages.Count} more messages!");
                if (messages.Count <= 0)
                {
                    break;
                }

                lastMessage = messages.Last();
                allMessages.AddRange(messages);
            }

            Console.WriteLine("done getting messages!");
            return allMessages;
        }
    }
}