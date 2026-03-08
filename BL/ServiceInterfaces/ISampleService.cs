using DotNet8Starter.Exceptions.Validations;

namespace DotNet8Starter.BL.ServiceInterfaces
{
	public interface ISampleService
	{
		Response GetEmptySuccess();
		
		Response<string> GetStringSuccess();
		
		Response GetError();
		
		Response GetErrorMessage();

		Response<string> GetErrorMessage2();

	}
}
