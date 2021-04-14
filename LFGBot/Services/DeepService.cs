using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord.WebSocket;
using LFGBot.Misc;
using Newtonsoft.Json;

namespace LFGBot.Services
{
    public class DeepService
    {
        private readonly Random _random;
        private readonly TimerPlus _messageTimer;
        private readonly TimerPlus _startTimer;
        private readonly Timer _consoleUpdateTimer;

        private HashSet<int> _usedPosts;
        private readonly List<(int hash, string msg)> _posts;

        public ISocketMessageChannel Channel = null!;

        private readonly DiscordSocketClient _client;
        private Config _config;

        private (string msg, int hash, int index)? _queuedMessage;
        
        public DeepService(DiscordSocketClient client, Config config)
        {
            _client = client;
            _config = config;
            
            _posts = new List<(int, string)>();
            _usedPosts = new HashSet<int>();
            _random = new Random();
            _messageTimer = new TimerPlus
            {
                AutoReset = true
            };
            
            _startTimer = new TimerPlus
            {
                AutoReset = false
            };
            
            _consoleUpdateTimer = new Timer
            {
                AutoReset = true,
                Interval = TimeSpan.FromSeconds(1).TotalMilliseconds
            };
            
            _messageTimer.Elapsed += MessageTimerOnElapsed;
            _startTimer.Elapsed += OnStartTimerElapsed;
            _consoleUpdateTimer.Elapsed += UpdateConsole;
            _consoleUpdateTimer.Start();

            var statsJson = File.ReadAllText(config.StatsPath);
            var stats = JsonConvert.DeserializeObject<BotStats>(statsJson);
            
            _client.Ready += () =>
            {
                LoadNewConfig(config);
                return Task.CompletedTask;
            };
        }

        private void OnStartTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _messageTimer.Start();
            _startTimer.Stop();
            
            SendDeepMessage().RunSynchronously();
        }

        public void LoadNewConfig(Config config)
        {
            _config = config;
            ToggleTimer(false);
            StartService();
        }

        private void StartService()
        {
            Channel = (ISocketMessageChannel) _client.GetChannel(_config.DeepPostChannel);
            
            LoadTexts();
            QueueMessage();

            _messageTimer.Interval = _config.DeepPostInterval.TotalMilliseconds;

            var startHour = _config.DeepPostStartTime.Hours;
            var startMinute = _config.DeepPostStartTime.Minutes;
            var startSecond = _config.DeepPostStartTime.Seconds;

            if (startHour + startMinute == 0)
            {
                _messageTimer.Start();
            }
            else
            {
                var now = DateTime.Now;
                if (startHour != 0)
                    now = startHour < now.Hour ? now.AddDays(1) : now;

                var startTime = new DateTime(now.Year, now.Month, now.Day, startHour == 0 ? startMinute < now.Minute ? now.Hour + 1 : now.Hour : startHour, startMinute, startSecond);
                var startDelay = (startTime - DateTime.Now).TotalMilliseconds;

                _startTimer.Interval = startDelay;

                _startTimer.Start();
            }
        }
        
        public void StopService()
        {
            ToggleTimer(false);

            if (_usedPosts.Any())
            {
                _queuedMessage = null;

                using StreamWriter file = File.CreateText(_config.UsedPostsPath);

                var serializer = new JsonSerializer();
                serializer.Serialize(file, _usedPosts);
            }
        }
        
        private void LoadTexts()
        {
            var text = new List<string>();
         
            _usedPosts.Clear();
            _posts.Clear();
            
            _usedPosts = JsonConvert.DeserializeObject<HashSet<int>>(File.ReadAllText(_config.UsedPostsPath));

            var texts = new DirectoryInfo(_config.DeepTextsPath).GetFiles().Where(f => f.Extension == ".txt");
            foreach (var file in texts)
            {
                text.AddRange(File.ReadAllLines(file.FullName));
            }

            var lineNum = 0;
            while (lineNum < text.Count)
            {
                var line = text[lineNum];

                if (line.StartsWith("==="))
                {
                    var msg = "";

                    for (var i = 1; lineNum + i < text.Count && !text[lineNum + i].StartsWith("==="); ++i)
                    {
                        msg += text[lineNum + i] + Environment.NewLine;
                    }

                    var hash = msg.GetDeterministicHashCode();
                    if (!_usedPosts.Contains(hash))
                        _posts.Add((hash, msg));
                }
                
                ++lineNum;
            }
        }

        public async Task SendDeepMessage()
        {
            if (!_queuedMessage.HasValue)
            {
                Console.WriteLine("Trying to send deep message but there is no queued message");
                return;
            }

            _posts.RemoveAt(_queuedMessage.Value.index);
            _usedPosts.Add(_queuedMessage.Value.hash);

            await Channel.SendMessageAsync(_queuedMessage.Value.msg);
            
            QueueMessage();
        }
        
        private void MessageTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            SendDeepMessage().RunSynchronously();
        }
        
        public void ToggleTimer(bool on)
        {
            _messageTimer.Enabled = on;
        }
        
        private void UpdateConsole(object sender, ElapsedEventArgs e)
        {
            if (_queuedMessage is null)
                return;
            
            Console.Clear();
            Console.WriteLine(_queuedMessage.Value.msg);
            
            if (_startTimer.Enabled && _startTimer.TimeLeft > 0)
            {
                Console.WriteLine($"Starting in {TimeSpan.FromMilliseconds(_startTimer.TimeLeft):dd\\:hh\\:mm\\:ss}");
            }
            
            if (_messageTimer.Enabled)
            {
                Console.WriteLine(
                    $"Sending in {TimeSpan.FromMilliseconds(_messageTimer.TimeLeft):dd\\:hh\\:mm\\:ss}"
                );
            }
        }
        
        public void QueueMessage(bool addNewline = false)
        {
            if (_posts.Count == 0)
            {
                _queuedMessage = ("[Warning] OUT OF PRE-GENERATED MESSAGES. PLEASE PING <@192875089165942784>.", 0, 0);
                return;
            }

            var index = _random.Next(_posts.Count - 1);
            var (hash, msg) = _posts[index];

            var finalMsg = "";
            
            for (var i = 0; i < msg.Length; i++)
            {
                var character = msg[i];

                // emoji
                if (character == ':')
                {
                    var emojiString = "";

                    var found = false;
                    
                    for (var j = i + 1; j < msg.Length; j++)
                    {
                        if (msg[j] == ':' && j > i + 1)
                        {
                            var emote = ((SocketGuildChannel) Channel).Guild.Emotes.FirstOrDefault(x => x.Name == emojiString);
                            if (emote != null)
                            {
                                finalMsg += $"<:{emote.Name}:{emote.Id}>";
                                i = j;
                                found = true;
                            }

                            break;
                        }

                        emojiString += msg[j];
                    }
                    if (!found)
                        finalMsg += character;
                }
                else
                {
                    finalMsg += character;
                }
            }

            if (addNewline)
                finalMsg += "​";
            
            _queuedMessage = (finalMsg, hash, index);
        }
        
    }
}