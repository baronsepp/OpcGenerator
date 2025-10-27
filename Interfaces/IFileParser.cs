using System.Collections.Generic;
using System.Threading;
using NodeGenerator.Models;

namespace NodeGenerator.Interfaces
{
    public interface IFileParser
    {
        IAsyncEnumerable<NodeModel> ParseAsync(string path, CancellationToken stoppingToken);
    }
}
