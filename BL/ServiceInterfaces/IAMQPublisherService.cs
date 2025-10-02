namespace DotNet8Starter.BL.ServiceInterfaces
{
	public interface IAMQPublisherService
	{
		bool SendMessage<T>(string eventName, T payload, int occurrences = 1);

	}
}
