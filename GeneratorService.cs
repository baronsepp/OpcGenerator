using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodeGenerator.Interfaces;
using NodeGenerator.Models;
using NodeGenerator.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NodeGenerator
{
    public class GeneratorService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IFileParser _fileParser;
        private readonly IFileWriter _fileWriter;
        private readonly ILogger<GeneratorService> _logger;
        private readonly OpcOptions _opcOptions;
        private readonly string _fileName;

        public GeneratorService(
            IConfiguration configuration,
            IFileParser fileParser,
            IFileWriter fileWriter,
            ILogger<GeneratorService> logger,
            IOptions<OpcOptions> opcOptions)
        {
            _fileName = configuration.GetValue<string>("InputFileName") ?? throw new ArgumentNullException(nameof(configuration));
            _fileParser = fileParser ?? throw new ArgumentNullException(nameof(fileParser));
            _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _opcOptions = opcOptions.Value ?? throw new ArgumentNullException(nameof(opcOptions));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var fullPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + _fileName;
            _logger.LogInformation($"Searching for {fullPath}");

            if (File.Exists(fullPath) is false)
            {
                _logger.LogWarning($"{fullPath} not found!");
                return;
            }

            _logger.LogInformation($"Found {fullPath}");

            var nodes = _fileParser.ParseAsync(fullPath, stoppingToken);
            var endpoints = await CreateEndpointAsync(nodes, stoppingToken);
            await _fileWriter.Write(endpoints);
        }

        private async Task<IReadOnlyCollection<EndpointModel>> CreateEndpointAsync(IAsyncEnumerable<NodeModel> enumerable, CancellationToken cancellationToken)
        {
            var collectionBuilder = new ReadOnlyCollectionBuilder<NodeModel>();

            await foreach (var node in enumerable.WithCancellation(cancellationToken))
            {
                collectionBuilder.Add(node);
            }

            var endpointModel = new EndpointModel
            {
                EndpointUrl = _opcOptions.EndpointUrl,
                UseSecurity = _opcOptions.UseSecurity,
                OpcNodes = collectionBuilder.ToReadOnlyCollection()
            };

            _logger.LogInformation($"Added nodes to {endpointModel}");
            return new List<EndpointModel>(1) { endpointModel };
        }
    }
}
