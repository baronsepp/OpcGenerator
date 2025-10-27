using NodeGenerator.Application.Models;

namespace NodeGenerator.Application.Abstractions;

public interface IFileWriter
{
	public Task WriteAsync(IReadOnlyCollection<EndpointModel> endpointModels);
}
