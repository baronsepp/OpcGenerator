using Microsoft.Extensions.Options;
using NodeGenerator.Interfaces;
using NodeGenerator.Models;
using NodeGenerator.Options;
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
        private readonly XmlOptions _xmlOptions;
        private readonly OpcOptions _opcOptions;

        public XmlParser(IOptions<XmlOptions> xmlOptions, IOptions<OpcOptions> opcOptions)
        {
            _xmlOptions = xmlOptions.Value;
            _opcOptions = opcOptions.Value;
        }

        public IAsyncEnumerable<NodeModel> ParseAsync(string path, CancellationToken stoppingToken)
        {
            return ReadFromXml(path, stoppingToken);
        }

        private async IAsyncEnumerable<NodeModel> ReadFromXml(string path, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var settings = new XmlReaderSettings
            {
                Async = true,
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            await using (var fileStream = File.OpenRead(path))
            using (var reader = XmlReader.Create(fileStream, settings))
            {
                reader.ReadToFollowing(_xmlOptions.NodeName);
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
                        OpcSamplingInterval = _opcOptions.SampleInterval,
                        OpcPublishingInterval = _opcOptions.PublishInterval,
                        DisplayName = displayName
                    };

                } while (cancellationToken.IsCancellationRequested is false && reader.ReadToFollowing(_xmlOptions.NodeName));
            }
        }
    }
}
