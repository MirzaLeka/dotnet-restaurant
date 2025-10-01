namespace DotNet8Starter.BL.ServiceInterfaces
{
	public interface IAMQPublisherService
	{
		void SendOrder(string order);
	}
}
