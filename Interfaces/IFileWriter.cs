using NodeGenerator.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NodeGenerator.Interfaces
{
    public interface IFileWriter
    {
        public Task Write(IReadOnlyCollection<EndpointModel> endpointModels);
    }
}
