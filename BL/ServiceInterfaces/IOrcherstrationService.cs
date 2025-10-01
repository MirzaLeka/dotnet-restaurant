namespace DotNet8Starter.BL.ServiceInterfaces
{
	public interface IOrcherstrationService
	{
		Task OrcherstrateEvents(string message, string? correlationId);
	}
}
