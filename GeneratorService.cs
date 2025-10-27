using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodeGenerator.Interfaces;
using NodeGenerator.Models;
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

        public GeneratorService(
            IConfiguration configuration,
            IFileParser fileParser,
            IFileWriter fileWriter,
            ILogger<GeneratorService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _fileParser = fileParser ?? throw new ArgumentNullException(nameof(fileParser));
            _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var fileName = _configuration.GetValue<string>("InputFileName");
            var fullPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + fileName;
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
                EndpointUrl = _configuration.GetValue<string>("OPC:EndpointUrl"),
                UseSecurity = _configuration.GetValue<bool>("OPC:UseSecurity"),
                OpcNodes = collectionBuilder.ToReadOnlyCollection()
            };

            _logger.LogInformation($"Added nodes to {endpointModel}");
            return new List<EndpointModel>(1) { endpointModel };
        }
    }
}
