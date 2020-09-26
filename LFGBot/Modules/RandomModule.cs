using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.IO;
using System.Timers;
using Discord.Rest;
using Discord.WebSocket;

namespace LFGBot.Modules
{
    public class RandomModule : ModuleBase<SocketCommandContext>
    {
        private readonly FileInfo[] _files;
        private readonly Random _random;
        private readonly Timer _timer;
        private ISocketMessageChannel _channel;
        
        
        public RandomModule()
        {
            _files = new DirectoryInfo(@"C:\Users\Rewind\Desktop\LFGMediaCollection").GetFiles();
            _random = new Random();
            _timer = new Timer(5000)
            {
                AutoReset = true
            };
            _timer.Elapsed += TimerOnElapsed;
            _timer.Enabled = true;
            
            //_timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (_channel is null)
                return;
            
            Console.WriteLine("deeeeeeeeeeez");
            _channel.SendMessageAsync(
                "Message"
            ).RunSynchronously();
            
        }

        [Command("setchannel")]
        [RequireOwner]
        public async Task SetChannel()
        {
            _channel = Context.Channel;
            await Context.Channel.SendMessageAsync($"Bot message channel set to {Context.Channel.Name}");
        }
        
        [Command("random")]
        [Summary("Sends a random piece of media that has been posted in lfg")]
        public async Task RandomImage()
        {
            var index = _random.Next(0, _files.Length - 1);

            //var stream = _files[index].OpenRead();
            
            //var image = new Image(stream);

            //var image = _files[index].
            
            await Context.Channel.SendFileAsync(_files[index].FullName);
        }
    }
}