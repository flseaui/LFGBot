using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LFGBot
{
    public class Config
    {
        public static Config LoadConfig(string path)
        {
            var configJson = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Config>(configJson);
        }

        public string TestBotToken { get; init; }
        public string MainBotToken { get; init; }

        public bool TestBot { get; init; }
        
        [JsonIgnore]
        public string CurrentToken => TestBot ? TestBotToken : MainBotToken;
        
        public string StatsPath { get; init; }
        public string DeepTextsPath { get; init; }
        public string UsedPostsPath { get; init; }
        
        public ulong DeepPostChannel { get; set; }

        public TimeSpan DeepPostInterval { get; set; } = TimeSpan.FromHours(3);
        public TimeSpan DeepPostStartTime { get; set; } = new TimeSpan(3, 47, 0);
    }
}