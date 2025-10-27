using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodeGenerator.Interfaces;
using NodeGenerator.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace NodeGenerator.Writers
{
    public class JsonWriter : IFileWriter
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<JsonWriter> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public JsonWriter(IConfiguration configuration, IHostEnvironment environment, ILogger<JsonWriter> logger)
        {
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }

        public async Task Write(IReadOnlyCollection<EndpointModel> endpointModels)
        {
            var outputFileName = _configuration.GetValue<string>("OutputFileName");

            using (var fileStream = File.Create(outputFileName))
            {
                await JsonSerializer.SerializeAsync(fileStream, endpointModels, JsonOptions);
            }

            _logger.LogInformation($"Finished writing to {outputFileName} in {_environment.ContentRootPath}");
        }
    }
}
