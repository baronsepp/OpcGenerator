using NodeGenerator.Application.Models;

namespace NodeGenerator.Application.Abstractions;

public interface IFileParser
{
	IAsyncEnumerable<NodeModel> ParseAsync(string path, CancellationToken stoppingToken);
}
