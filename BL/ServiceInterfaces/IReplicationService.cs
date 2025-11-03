namespace DotNet8Starter.BL.ServiceInterfaces
{
	public interface IReplicationService
	{
		Task<bool> GetReplicationStatus();
	}
}
