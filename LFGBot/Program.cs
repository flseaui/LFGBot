namespace LFGBot
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args != null)
            {
                Initialize.TestBot = args[0] == "--test";
            }

            new Initialize().MainAsync().GetAwaiter().GetResult();
        }
    }
}
