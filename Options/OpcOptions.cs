namespace NodeGenerator.Options
{
    public class OpcOptions
    {
        public const string Section = "OPC";

        public string EndpointUrl { get; set; }
        public bool UseSecurity { get; set; }
        public int PublishInterval { get; set; }
        public int SampleInterval { get; set; }
    }
}