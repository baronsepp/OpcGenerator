using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodeGenerator.Interfaces;
using NodeGenerator.Models;
using NodeGenerator.Options;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NodeGenerator.Parsers
{
    public class CsvParser : IFileParser
    {
        private readonly CsvOptions _csvOptions;
        private readonly OpcOptions _opcOptions;
        private readonly ILogger<CsvParser> _logger;

        public CsvParser(IOptions<CsvOptions> csvOptions, IOptions<OpcOptions> opcOptions, ILogger<CsvParser> logger)
        {
            _csvOptions = csvOptions.Value;
            _opcOptions = opcOptions.Value;
            _logger = logger;
        }

        public IAsyncEnumerable<NodeModel> ParseAsync(string path, CancellationToken stoppingToken = default)
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
            var skipFirst = _csvOptions.SkipFirst;
            await foreach (var line in csvLines.WithCancellation(stoppingToken))
            {

                if (skipFirst is true)
                {
                    skipFirst = false;
                    continue;
                }

                var values = line.Replace("\"", "").Split(_csvOptions.Delimiter);

                yield return new NodeModel
                {
                    Id = values[_csvOptions.NodeIndex],
                    OpcPublishingInterval = _opcOptions.PublishInterval,
                    OpcSamplingInterval = _opcOptions.SampleInterval,
                    DisplayName = values[_csvOptions.DisplayNameIndex]
                };
            }
        }
    }
}
