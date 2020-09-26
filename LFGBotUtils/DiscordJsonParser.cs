using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LFGBotUtils
{
    public class DiscordJsonParser
    {
        static void Main(string[] args)
        {

            var messages = new List<string>();
            
            var queuedFiles = Directory.EnumerateFiles("../../../TargetJson", "*.json").Select(File.ReadAllText).ToList();

            using (var outputFile = new StreamWriter(Path.Combine("../../../", "outputMessages.txt")))
            {
                foreach (var json in queuedFiles)
                {
                    var jsonObj = JObject.Parse(json);

                    var bodies = new List<string>();

                    foreach (var msg in jsonObj["messages"]) //.Children<JArray>())
                    {
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
            }


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
}