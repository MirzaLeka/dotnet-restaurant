using DotNet8Starter.Exceptions.Validations;

namespace DotNet8Starter.BL.ServiceInterfaces
{
	public interface IResultExamplesService
	{
		Result<string> GetEmpty();

		Response<string> GetResponseAsString(int id);
		Response<WeatherForecast> GetResponseAsWeather(WeatherForecast forecast);
		Response GetResponse(int id);
		Response GetEmptyResponse(int id);
		Task<Response<WeatherForecast>> GetCreatedResponse();
		//Response CheckErrorObj();
		Response<string> CheckErrorObj();

	}
}
