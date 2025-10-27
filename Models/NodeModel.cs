namespace NodeGenerator.Models
{
    public struct NodeModel
    {
        public string Id { get; set; }
        public int OpcSamplingInterval { get; set; }
        public int OpcPublishingInterval { get; set; }
        public string DisplayName { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
