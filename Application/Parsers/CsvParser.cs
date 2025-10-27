using Microsoft.Extensions.Options;
using NodeGenerator.Application.Models;
using NodeGenerator.Application.Abstractions;
using NodeGenerator.Options;
using System.Runtime.CompilerServices;

namespace NodeGenerator.Application.Parsers;

public sealed class CsvParser : IFileParser
{
	private readonly CsvOptions _csvOptions;
	private readonly OpcOptions _opcOptions;

	public CsvParser(IOptions<CsvOptions> csvOptions, IOptions<OpcOptions> opcOptions)
	{
		_csvOptions = csvOptions.Value ?? throw new ArgumentNullException(nameof(csvOptions));
		_opcOptions = opcOptions.Value ?? throw new ArgumentNullException(nameof(opcOptions));
	}

	public IAsyncEnumerable<NodeModel> ParseAsync(string path, CancellationToken stoppingToken = default)
	{
		var fileLines = ReadFromCsvAsync(path, stoppingToken);
		return ParseLineAsync(fileLines, stoppingToken);
	}

	private static async IAsyncEnumerable<string> ReadFromCsvAsync(string path, [EnumeratorCancellation] CancellationToken stoppingToken = default)
	{
		await using (var stream = File.OpenRead(path))
		using (var reader = new StreamReader(stream))
		{
			while (stoppingToken.IsCancellationRequested is false && reader.EndOfStream is false)
			{
				yield return await reader.ReadLineAsync(stoppingToken) ?? throw new NullReferenceException(nameof(reader));
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
