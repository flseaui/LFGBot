using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LFGBotUtils
{
    public static class DiscordJsonParser
    {
        private static void Main(string[] args)
        {
            var file = File.ReadAllText("../../../Misc/tweet.json");

            using var outputFile = new StreamWriter(Path.Combine("../../../Output/", "outputTweets.txt"), false);
            dynamic jsonObj = JsonConvert.DeserializeObject<JObject>(file);

            foreach (var tweet in jsonObj.tweets)
            {
                if (tweet.retweeted is object rtd && (bool) rtd)
                    continue;

                var tweetTxt = (string) tweet.tweet.full_text;
                    
                if (tweetTxt == "") continue;
                
                var startIndex = 0;

                var rt = tweetTxt.StartsWith("RT");
                if (tweetTxt[0] == '@' || rt)
                {
                    var start = rt ? 3 : 0;
                    startIndex = start;
                    for (var i = start; i < tweetTxt.Length; i++)
                    {
                        var character = tweetTxt[i];

                        if (character == '@')
                        {
                            for (var j = i + 1; j < tweetTxt.Length; j++)
                            {
                                if (tweetTxt[j] != ' ' || j <= i + 1) continue;
                                
                                startIndex = j + 1;
                                i = j;
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                //if (tweetTxt == "" && tweetTxt == " " && tweetTxt != Environment.NewLine) continue;
                        
                try
                {
                    outputFile.WriteLine($"<|startoftext|> {tweetTxt.Substring(startIndex)} <|endoftext|>");
                }
                catch (EncoderFallbackException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            outputFile.Close();
        }

        /*static void Main(string[] args)
        {

            var carlFilter = true;
            
            var messages = new List<string>();
            
            var queuedFiles = Directory.EnumerateFiles("../../../TargetJson", "*.json").Select(File.ReadAllText).ToList();

            using (var outputFile = new StreamWriter(Path.Combine("../../../", "outputMessages.txt")))
            {
                foreach (var json in queuedFiles)
                {
                    var jsonObj = JObject.Parse(json);

                    var bodies = new List<string>();

                    var channel = 
                    
                    foreach (var msg in jsonObj["messages"]) //.Children<JArray>())
                    {
                        if (!msg["author"]["isBot"].ToObject<bool>()) continue;
                        
                        var content = msg["content"].ToString();
                        if (content != "" && !content.StartsWith("https"))
                        {
                            outputFile.WriteLine(msg["content"]);
                            //outputFile.WriteLine("<|startoftext|>" + msg["content"] + "<|endoftext|>");
                        }

                        //messages.Add(msg["content"].ToString());
                        //Console.WriteLine(msg["content"]);
                    }
                }
                outputFile.Close();
            }*/
        
           //var outputJson = JsonConvert.SerializeObject(messages);
            
            //File.WriteAllText("../../../outputMessages.txt", outputJson);
            
            /*using var sw = new StreamWriter("OutputCsv.csv");
            
            var csvWriter = new CsvWriter(TextWriter.Synchronized(sw), CultureInfo.InvariantCulture);

            foreach (var body in bodies)
            {
                csvWriter.WriteField($"[{body}]");
                csvWriter.NextRecord();
            }
            
            csvWriter.Dispose();
            sw.Close();*/
    }
}