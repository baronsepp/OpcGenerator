using Microsoft.Extensions.Configuration;

using NodeGenerator.Interfaces;
using NodeGenerator.Models;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Xml;

namespace NodeGenerator.Parsers
{
    public class XmlParser : IFileParser
    {
        private readonly IConfiguration _configuration;

        public XmlParser(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IAsyncEnumerable<NodeModel> ParseAsync(string path, CancellationToken stoppingToken)
        {
            return ReadFromXml(path, stoppingToken);
        }

        private async IAsyncEnumerable<NodeModel> ReadFromXml(string path, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var nodeName = _configuration.GetValue<string>("XML:NodeName");
            var settings = new XmlReaderSettings
            {
                Async = true,
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            var sampleInterval = _configuration.GetValue<int>("OPC:SampleInterval");
            var publishInterval = _configuration.GetValue<int>("OPC:PublishInterval");

            await using (var fileStream = File.OpenRead(path))
            using (var reader = XmlReader.Create(fileStream, settings))
            {
                reader.ReadToFollowing(nodeName);
                do
                {
                    reader.MoveToAttribute("NodeId");
                    var sb = new StringBuilder(await reader.GetValueAsync());
                    var id = sb.Replace("&quot;", "\"").ToString();

                    reader.ReadToFollowing("DisplayName");
                    var displayName = await reader.ReadElementContentAsStringAsync();

                    yield return new NodeModel
                    {
                        Id = id,
                        OpcSamplingInterval = sampleInterval,
                        OpcPublishingInterval = publishInterval,
                        DisplayName = displayName
                    };

                } while (cancellationToken.IsCancellationRequested is false && reader.ReadToFollowing(nodeName));
            }
        }
    }
}
