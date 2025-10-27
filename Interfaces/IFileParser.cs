using NodeGenerator.Models;
using System.Collections.Generic;
using System.Threading;

namespace NodeGenerator.Interfaces
{
    public interface IFileParser
    {
        IAsyncEnumerable<NodeModel> ParseAsync(string path, CancellationToken stoppingToken);
    }
}
