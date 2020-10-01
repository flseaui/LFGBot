using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Discord.WebSocket;

namespace LFGBot.Services
{
    public class DeepService
    {
        private readonly Random _random;
        private readonly Timer _timer;
        
        private FileInfo[] _texts;
        
        private List<(int hash, string msg)> _usedQuotes;
        private List<(int hash, string msg)> _quotes;

        public ISocketMessageChannel Channel;

        public int Interval;
        public int NumMessagesSent;
        public int NumImagesSent;
        
        public DeepService()
        {
            _quotes = new List<(int, string)>();
            _usedQuotes = new List<(int, string)>();
            _random = new Random();
            _texts = new DirectoryInfo(@"C:\Users\Rewind\Desktop\LFGDeepTexts").GetFiles();

            var infoText = File.ReadAllLines(@"C:\Users\Rewind\Desktop\LFGBotAdmin\LFGBotInfo.txt");
            Interval = int.Parse(infoText[0]);
            NumMessagesSent = int.Parse(infoText[1]);
            NumImagesSent = int.Parse(infoText[2]);
            
            _timer = new Timer(Interval)
            {
                AutoReset = true
            };
            _timer.Elapsed += TimerOnElapsed;
            _timer.Enabled = false;

            var text = new List<string>();
            
            foreach (var file in _texts)
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

                    _quotes.Add((msg.GetHashCode(), msg));
                }
                
                ++lineNum;
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            Channel?.SendMessageAsync(
                GetMessage()
            ).RunSynchronously();
        }

        public void ToggleTimer(bool on)
        {
            _timer.Enabled = on;
        }
        
        public string GetMessage()
        {
            if (_quotes.Count == 0)
                return "[Warning] OUT OF PRE-GENERATED MESSAGES. PLEASE PING <@192875089165942784>.";
            
            var index = _random.Next(_quotes.Count - 1);
            var quote = _quotes[index];

            _usedQuotes.Add(quote);
            _quotes.RemoveAt(index);

            return quote.msg;
        }
        
    }
}