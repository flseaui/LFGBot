using System;
using System.IO;

namespace LFGBot
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string? configPath = null;
            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--config" or "-c":
                        if (args.Length <= i + 1)
                        {
                            Console.Error.WriteLine($"{args[i]} must be followed by a valid path");
                            return;
                        }
                        try
                        {
                            Path.GetFullPath(args[++i]);
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine($"{args[i - 1]} was followed by an invalid path");
                            Console.WriteLine(e);
                            return;
                        }

                        configPath = args[i];
                        break;
                }
            }

            if (configPath is null)
            {
                Console.Error.WriteLine("--config/-c argument must be supplied");
                return;
            }
            
            new Initialize(Config.LoadConfig(configPath)).MainAsync().GetAwaiter().GetResult();
        }
    }
}
