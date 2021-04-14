using Discord.Commands;
using LFGBot.Services;

namespace LFGBot.Modules
{
    public class StatsModule : ModuleBase<SocketCommandContext>
    {
        private readonly StatsService _statsService;
        
        public StatsModule(StatsService statsService)
        {
            _statsService = statsService;
        }
    }
}