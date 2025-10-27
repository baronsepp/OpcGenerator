using Microsoft.Extensions.Configuration;
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
        private readonly ILogger<JsonWriter> _logger;

        public JsonWriter(IConfiguration configuration, ILogger<JsonWriter> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task Write(IList<EndpointModel> endpointModels)
        {
            var outputFileName = _configuration.GetValue<string>("OutputFileName");
            var options = new JsonSerializerOptions { WriteIndented = true };

            using (var fileStream = File.Create(outputFileName))
            {
                await JsonSerializer.SerializeAsync(fileStream, endpointModels, options);
            }

            _logger.LogInformation($"Finished writing to {outputFileName} in {Directory.GetCurrentDirectory()}");
        }
    }
}
