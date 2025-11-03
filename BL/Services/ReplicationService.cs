using DotNet8Starter.BL.ServiceInterfaces;

namespace DotNet8Starter.BL.Services
{
	public class ReplicationService : IReplicationService
	{
		/// <summary>
		/// Returns random replication status for demo purposes
		/// </summary>
		/// <returns></returns>
		public async Task<bool> GetReplicationStatus()
		{
			return Random.Shared.Next(2) == 0;
		}

	}
}
