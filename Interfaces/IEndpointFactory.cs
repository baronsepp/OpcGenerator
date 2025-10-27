using NodeGenerator.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NodeGenerator.Interfaces
{
    public interface IEndpointFactory
    {
        public Task<IList<EndpointModel>> CreateAsync(IAsyncEnumerable<NodeModel> nodes, CancellationToken stoppingToken = default);

        public IList<EndpointModel> Create(IEnumerable<NodeModel> nodes);
    }
}
