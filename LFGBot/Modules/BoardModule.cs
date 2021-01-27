using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LFGBot.Services;

namespace LFGBot.Modules
{
    [Group("board")]
    public class BoardModule : ModuleBase<SocketCommandContext>
    {
        private readonly BoardService _boardService;

        public BoardModule(BoardService deepService)
        {
            _boardService = deepService;
        }

        [Command("create")]
        public async Task CreateBoard()
        {
            
        }
        
        [Command("quote")]
        public async Task RandomMessage()
        {
            var message = _boardService.GetUniqueRandomMessage(729759705190367273);

            if (message.Embeds.Any())
            {
                var embed = message.Embeds.First() as Embed;
                
                await Context.Channel.SendMessageAsync(message.Content, false, embed);
            }
            else
            { 
                var builder = new EmbedBuilder();

                var guild = ((SocketGuildChannel) Context.Client.GetChannel(729759705190367273)).Guild;
                var user = guild.GetUser(message.Author.Id);

                var avatarUrl = message.Author.GetAvatarUrl(ImageFormat.Png);
                var name = user != null ? user.Nickname : message.Author != null ? message.Author.Username : "invalid-user";
                
                builder.WithAuthor(name, avatarUrl);

                builder.WithDescription(message.Content);
                builder.WithFooter($"{message.Id} • {message.Timestamp.Date.ToShortDateString()}");
                
                if (message.Attachments.Any())
                {
                    var attachment = message.Attachments.First();
                    if (attachment.Filename.EndsWith(".png") || attachment.Filename.EndsWith(".jpg"))
                        builder.WithImageUrl(attachment.Url);
                    else
                        builder.WithUrl(attachment.Url);
                }
                
                builder.WithColor(Color.Purple);
                var embed = builder.Build();
                
                await Context.Channel.SendMessageAsync("", false, embed);
            }
        }
    }
}