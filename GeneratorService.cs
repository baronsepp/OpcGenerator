using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodeGenerator.Interfaces;

namespace NodeGenerator
{
    public class GeneratorService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IEndpointFactory _endpointFactory;
        private readonly IFileParser _fileParser;
        private readonly IFileWriter _fileWriter;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger<GeneratorService> _logger;

        public GeneratorService(
            IConfiguration configuration, 
            IEndpointFactory endpointFactory, 
            IFileParser fileParser, 
            IFileWriter fileWriter, 
            IHostApplicationLifetime lifetime,
            ILogger<GeneratorService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _fileParser = fileParser ?? throw new ArgumentNullException(nameof(fileParser));
            _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
            _endpointFactory = endpointFactory ?? throw new ArgumentNullException(nameof(endpointFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
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
            var endpoints = await _endpointFactory.CreateAsync(nodes, stoppingToken);
            await _fileWriter.Write(endpoints);

            _lifetime.StopApplication();
        }
    }
}
