namespace DotNet8Starter.BL.ServiceInterfaces
{
	public interface ILogOrcherstrationExceptionsService
	{
		/// TODO i sve ostalo sto ce ici u ovu tabelu
		public Task LogAMQException(string message, string? correlationId);
		public Task LogHTTPException(string message, string? correlationId);
	}
}
