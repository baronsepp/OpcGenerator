namespace NodeGenerator.Application.Models;

public sealed class EndpointModel
{
	public string EndpointUrl { get; init; } = "opc.tcp://127.0.0.1:4840";
	public bool UseSecurity { get; init; }
	public IReadOnlyCollection<NodeModel> OpcNodes { get; init; } = new List<NodeModel>(0);

	public override string ToString()
	{
		return EndpointUrl;
	}
}
