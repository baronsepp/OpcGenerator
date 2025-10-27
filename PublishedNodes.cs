using System.Collections.Generic;

namespace NodeGenerator
{
    public class PublishedNode
    {
        public string EndpointUrl { get; set; } = "opc.tcp://127.0.0.1:49320";
        public bool UseSecurity { get; set; }
        public IList<Node> OpcNodes { get; set; } = new List<Node>();
    }

    public class Node
    {
        public string Id { get; set; }
        public int OpcSamplingInterval { get; set; } = 1000;
        public int OpcPublishingInterval { get; set; } = 1000;
    }
}
