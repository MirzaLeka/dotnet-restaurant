using DotNet8Starter.BL.ServiceInterfaces;
using DotNet8Starter.Exceptions.Validations;
using System.Net;

namespace DotNet8Starter.BL.Services
{
	public class SampleService : ISampleService
	{
		public Response GetEmptySuccess()
		{
			return Response.Success();
		}

		public Response<string> GetStringSuccess()
		{
			return Response<string>.Success("blah!");
		}

		public Response<string> GetStringCreated()
		{
			return Response<string>.Created("this is cool!");
		}

		// Test this
		//  public static new Response<T> Empty() => (Response<T>)Response.Empty();
		public Response<string> GetStringEmpty()
		{
			return Response<string>.Empty();
		}

		public Response GetError()
		{
			return Response.Error();
		}

		public Response GetErrorMessage()
		{
			return Response.Error("Something went wrong!");
		}

		public Response<string> GetErrorMessage2()
		{
			return Response<string>.Error("Something went wrong!");
		}

		public Response GetErrorAsHttp()
		{
			return Response.Error(400, "Sorry bro!");
		}

		public Response<WeatherForecast> GetErrorAsHttp2()
		{
			//return Response<WeatherForecast>.Error(400, "yes it works this way too!");
			return Response<WeatherForecast>.Error(HttpStatusCode.BadRequest, "Noo bro!");
		}

		public Response GetErrorAsHttp3()
		{
			return ResponseError.BadRequest("noooo!");
		}

		public Response<WeatherForecast> GetErrorAsHttp4()
		{
			return ResponseError.BadRequest("noooo!");
		}
	}
}
