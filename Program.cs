using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static NodeGenerator.NodeGenerator;

namespace NodeGenerator
{
    public static class Program
    {
        private static IEnumerable<string> Tags { get; set; }
        private static string Path { get; set; }
        
        public static void Main()
        {
            //Nsu = Ask("ExpandedNodeId");
            //Channel = Ask("Channel");
            //Device = Ask("Device");
            //Path = Ask("Path");

            var tags = File.ReadAllLines(Path, Encoding.UTF8)
                .Skip(1)
                .Select(FromCsv)
                .ToList();

            var publishedNode = new PublishedNode
            {
                EndpointUrl = Ask("Endpoint"), 
                UseSecurity = false,
                OpcNodes = tags
            };
            var publishedNodes = new List<PublishedNode> { publishedNode };

            var json = JsonConvert.SerializeObject(publishedNodes);
            var formattedJson = JToken.Parse(json).ToString(Formatting.Indented);
            using var writer = new StreamWriter("./publishednodes.json");
            writer.WriteLine(formattedJson);
            writer.Flush(); writer.Dispose();
        }

        private static string Ask(string input)
        {
            Console.Write($"Enter {input}: ");
            var answer = Console.ReadLine();
            return answer;
        }

        private static Node FromCsv(string csvLine)
        {
            var values = csvLine.Replace("\"", "").Split(',');
            var node = new Node { Id = Prefix + values[0] };
            return node;
        }
    }
}
