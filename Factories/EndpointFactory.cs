using Microsoft.Extensions.Configuration;
using NodeGenerator.Interfaces;
using NodeGenerator.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NodeGenerator.Factories
{
    public class EndpointFactory : IEndpointFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EndpointFactory> _logger;

        public EndpointFactory(IConfiguration configuration, ILogger<EndpointFactory> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IList<EndpointModel>> CreateAsync(IAsyncEnumerable<NodeModel> enumerable, CancellationToken cancellationToken)
        {
            var collectionBuilder = new ReadOnlyCollectionBuilder<NodeModel>();

            await foreach (var node in enumerable.WithCancellation(cancellationToken))
            {
                collectionBuilder.Add(node);
            }

            var endpointModel = new EndpointModel
            {
                EndpointUrl = _configuration.GetValue<string>("OPC:EndpointUrl"),
                UseSecurity = _configuration.GetValue<bool>("OPC:UseSecurity"),
                OpcNodes = collectionBuilder.ToReadOnlyCollection()
            };

            _logger.LogInformation($"Added nodes to {endpointModel}");
            return new List<EndpointModel>(1) { endpointModel };
        }

        public IList<EndpointModel> Create(IEnumerable<NodeModel> nodes)
        {
            throw new NotImplementedException();
        }
    }
}
