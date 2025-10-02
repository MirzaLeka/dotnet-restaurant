using DotNet8Starter.BL.ServiceInterfaces;

namespace DotNet8Starter.BL.Services
{
	public class LogOrcherstrationExceptionsService : ILogOrcherstrationExceptionsService
	{
		// add DB context here

		// TODO dodaj nekakvo mapiranje u DTO ako treba


		public Task LogAMQException(string message, string? correlationId)
		{
			throw new NotImplementedException();
		}

		public Task LogHTTPException(string message, string? correlationId)
		{
			throw new NotImplementedException();
		}
	}
}
