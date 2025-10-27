using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodeGenerator.Application.Models;
using NodeGenerator.Application.Abstractions;
using System.Text.Json;

namespace NodeGenerator.Application.Writers;

public class JsonWriter : IFileWriter
{
	private readonly string _outputFileName;
	private readonly IHostEnvironment _environment;
	private readonly ILogger<JsonWriter> _logger;

	private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

	public JsonWriter(IConfiguration configuration, IHostEnvironment environment, ILogger<JsonWriter> logger)
	{
		_outputFileName = configuration.GetValue<string>("OutputFileName") ?? throw new ArgumentNullException(nameof(configuration));
		_environment = environment ?? throw new ArgumentNullException(nameof(environment));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task WriteAsync(IReadOnlyCollection<EndpointModel> endpointModels)
	{
		using (var fileStream = File.Create(_outputFileName))
		{
			await JsonSerializer.SerializeAsync(fileStream, endpointModels, _jsonOptions);
		}

		_logger.LogInformation("Finished writing to {OutputFileName} in {ContentRootPath}", _outputFileName, _environment.ContentRootPath);
	}
}
