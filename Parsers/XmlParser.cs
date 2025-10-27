using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NodeGenerator.Interfaces;
using NodeGenerator.Models;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml;

namespace NodeGenerator.Parsers
{
    public class XmlParser : IFileParser
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<XmlParser> _logger;

        public XmlParser(IConfiguration configuration, ILogger<XmlParser> logger)
        {
            _configuration = configuration;
            _logger = logger;
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

            await using (var fileStream = File.OpenRead(path))
            using (var reader = XmlReader.Create(fileStream, settings))
            {
                reader.ReadToFollowing(nodeName);
                do
                {
                    var nodeModel = new NodeModel();

                    reader.MoveToAttribute("NodeId");
                    nodeModel.Id = await reader.GetValueAsync();

                    reader.ReadToFollowing("DisplayName");
                    nodeModel.DisplayName = await reader.ReadElementContentAsStringAsync();

                    yield return nodeModel;

                } while (cancellationToken.IsCancellationRequested is false && reader.ReadToFollowing(nodeName));
            }
        }
    }
}
