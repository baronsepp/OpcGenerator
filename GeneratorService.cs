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
            _configuration = configuration;
            _fileParser = fileParser;
            _fileWriter = fileWriter;
            _endpointFactory = endpointFactory;
            _logger = logger;
            _lifetime = lifetime;
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
            
            var nodes = _fileParser.Parse(fullPath, stoppingToken);
            var endpoints = await _endpointFactory.CreateAsync(nodes, stoppingToken);
            await _fileWriter.Write(endpoints);

            _lifetime.StopApplication();
        }
    }
}
