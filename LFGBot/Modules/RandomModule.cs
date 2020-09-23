using System;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;

namespace LFGBot.Modules
{
    public class RandomModule : ModuleBase<SocketCommandContext>
    {
        private readonly FileInfo[] _files;
        
        public RandomModule()
        {
            _files = new DirectoryInfo("../../../../MediaCollection/").GetFiles();
        }

        [Command("random")]
        [Summary("Sends a random piece of media that has been posted in lfg")]
        public Task RandomImage()
        {
            var index = new Random().Next(0, _files.Length);
         
            return ReplyAsync(_files[index].Name);
        }
    }
}