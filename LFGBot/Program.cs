namespace LFGBot
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            new Initialize().MainAsync().GetAwaiter().GetResult();
        }
    }
}
