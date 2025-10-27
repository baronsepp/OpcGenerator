namespace NodeGenerator.Options;

public class OpcOptions
{
	public const string Section = "OPC";

	public string EndpointUrl { get; set; } = "opc.tcp://127.0.0.1:4840";
	public bool UseSecurity { get; set; }
	public int PublishInterval { get; set; }
	public int SampleInterval { get; set; }
}