using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodeGenerator.Application.Abstractions;
using NodeGenerator.Application.Models;
using NodeGenerator.Options;
using System.Runtime.CompilerServices;

namespace NodeGenerator;

public sealed class Generator
{
	private readonly IFileParser _fileParser;
	private readonly IFileWriter _fileWriter;
	private readonly ILogger<Generator> _logger;
	private readonly OpcOptions _opcOptions;
	private readonly string _fileName;

	public Generator(
		IConfiguration configuration,
		IFileParser fileParser,
		IFileWriter fileWriter,
		ILogger<Generator> logger,
		IOptions<OpcOptions> opcOptions)
	{
		_fileName = configuration.GetValue<string>("InputFileName") ?? throw new ArgumentNullException(nameof(configuration));
		_fileParser = fileParser ?? throw new ArgumentNullException(nameof(fileParser));
		_fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_opcOptions = opcOptions.Value ?? throw new ArgumentNullException(nameof(opcOptions));
	}

	public async Task GenerateAsync(CancellationToken stoppingToken = default)
	{
		var fullPath = _fileName;
		_logger.LogInformation("Searching for {Path}", fullPath);

		if (File.Exists(fullPath) is false)
		{
			_logger.LogWarning("{Path} not found!", fullPath);
			return;
		}

		_logger.LogInformation("Found {Path}", fullPath);

		var nodes = _fileParser.ParseAsync(fullPath, stoppingToken);
		var endpoints = await CreateEndpointAsync(nodes, stoppingToken);
		await _fileWriter.WriteAsync(endpoints);
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

		_logger.LogInformation("Added nodes to {EndpointModel}", endpointModel);
		return new List<EndpointModel>(1) { endpointModel };
	}
}
