using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NodeGenerator.Interfaces;
using NodeGenerator.Models;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NodeGenerator.Parsers
{
    public class CsvParser : IFileParser
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CsvParser> _logger;

        public CsvParser(IConfiguration configuration, ILogger<CsvParser> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public IAsyncEnumerable<NodeModel> Parse(string path, CancellationToken stoppingToken = default)
        {
            var fileLines = ReadFromCsv(path, stoppingToken);
            return ParseLineAsync(fileLines, stoppingToken);
        }

        private async IAsyncEnumerable<string> ReadFromCsv(string path, [EnumeratorCancellation] CancellationToken stoppingToken = default)
        {
            await using (var stream = File.OpenRead(path))
            using (var reader = new StreamReader(stream))
            {
                while (stoppingToken.IsCancellationRequested is false && reader.EndOfStream is false)
                {
                    yield return await reader.ReadLineAsync();
                }
            }
        }

        private async IAsyncEnumerable<NodeModel> ParseLineAsync(IAsyncEnumerable<string> csvLines, [EnumeratorCancellation] CancellationToken stoppingToken)
        {
            var delimiter = _configuration.GetValue<char>("CSV:Delimiter");
            var nodeIndex = _configuration.GetValue<int>("CSV:NodeIndex");
            var displayIndex = _configuration.GetValue<int>("CSV:DisplayNameIndex");
            var skipFirst = _configuration.GetValue<bool>("CSV:SkipFirst");

            await foreach (var line in csvLines.WithCancellation(stoppingToken))
            {
                if (skipFirst is true)
                {
                    skipFirst = false;
                    continue;
                }

                var values = line.Replace("\"", "").Split(delimiter);

                yield return new NodeModel
                {
                    Id = values[nodeIndex],
                    OpcPublishingInterval = _configuration.GetValue<int>("OPC:PublishInterval"),
                    OpcSamplingInterval = _configuration.GetValue<int>("OPC:SampleInterval"),
                    DisplayName = values[displayIndex]
                };
            }
        }
    }
}
