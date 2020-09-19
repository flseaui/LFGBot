using System;
using System.Threading.Tasks;

namespace LFGBot
{
    class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
        }
    }
}