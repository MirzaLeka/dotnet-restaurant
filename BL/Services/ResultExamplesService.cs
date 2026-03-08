using DotNet8Starter.BL.ServiceInterfaces;
using DotNet8Starter.Exceptions.Validations;

namespace DotNet8Starter.BL.Services
{
	public class ResultExamplesService : IResultExamplesService
	{
		private static readonly string[] Summaries = new[]
		{
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

		private Result<List<WeatherForecast>> GetWeather()
		{

			var list = new List<WeatherForecast> { 
				new () {
					Date = DateOnly.FromDateTime(DateTime.Now),
					TemperatureC = 32,
					Summary = "Very hot!"
				}
			};

			// some business logic

			// convert list to Result 

			var resList = Result<List<WeatherForecast>>.Success(list);

			// Override == update

			//resList.Update(resList);

			// Add

			resList = resList.Add(new WeatherForecast { TemperatureC = 100, Summary = "Oh no!" });

			return list;
		}

		public Response<string> CheckErrorObj()
		{
			return ResponseError.BadRequest("!!!!!");

			//return Response<string>.Success("HEllo");

			//return "Works either way!";
		}

		public Result<string> GetEmpty()
		{
			//return Result<string>.Success(string.Empty);

			//return Result<string>.Empty();

			// TODO dodaj logiku da kreiras Default() iz templejta
			// i onda ces moci varijable praviti od Result-ova
			// npr deklarises gore varijablu, apdejtuejes je i na kraju vratis

			// Start of method
			var res = Result<string>.New();

			// After some business logic
			res = res.Update("Hello");

			// More good stuff
			res = res.Append(" World");

			// Oh no, an error
			//return res
			//	.Convert(ResultStatus.BadRequestException, "error message!"); // status 400, error message!



			var taLista = GetWeather();

			var emptyL = taLista.Clear();

			Result<string> resultStr = Result<string>.Success("Hi");
			string value = resultStr; // value == "Hi" -- umjesto successfulReports.Value.Count mozes ici value.Count

			Result<int> resultInt = Result<int>.Success(123);

			if (resultInt.IsSuccessful)
			{
				resultInt = Result<int>.Success(444);
			}

			resultInt = resultInt.Update(999);

			var valueInt = resultInt; // valueInt == 123


			Result<string> errorResult = Result<string>.Error("Something went wrong");

			var updatedError = errorResult.Update("oh noes!");

			var validationError = Result<string>.BadRequestError("error message");

			//var notFoundError = validationError.Convert(ResultStatus.NotFoundException, validationError.ExceptionMessage);
			var notFoundError = validationError.Convert(ResultStatus.NotFoundException); // preserves the same error message


			var successOk = validationError
				.Convert(ResultStatus.Ok)
				.Append("Redoslijed ti nije bio dobar!")
				.Clear();

			successOk = successOk.Append("Come on man!");

			string successOkMsg = successOk;

			//string errMsg = errorResult; // value == "Hi"

			return successOkMsg;

		}

		// WORKS!
		public Response<string> GetResponseAsString(int id)
		{
			if (id < 1)
			{
				return ResponseError.Invalid("Invalid id", id);
			}

			return "Hello!";
		}

		// WORKS!
		public Response<WeatherForecast> GetResponseAsWeather(WeatherForecast forecast)
		{
			if (forecast.TemperatureC > 100)
			{
				return ResponseError.Invalid("Temperature is too high", forecast);
			}

			return new WeatherForecast
			{
				TemperatureC = forecast.TemperatureC,
				Date = DateOnly.FromDateTime(DateTime.Now),
				Summary = "its weather"
			};
		}

		// WORKS!
		public Response GetResponse(int id)
		{
			if (id == 400)
			{
				return ResponseError.Invalid("!");
			}
			if (id == 401)
			{
				return ResponseError.Unauthorized("!");
			}
			if (id == 403)
			{
				return ResponseError.Forbidden("!");
			}
			if (id == 404)
			{
				return ResponseError.NotFound("!");
			}
			if (id == 500)
			{
				return ResponseError.InternalError("!");
			}
			if (id > 599)
			{
				var ex = new Exception("Invalid id!");
				return ResponseError.InternalError(ex);
			}

			return Response.Success();
		}

		// WORKS!
		public Response GetEmptyResponse(int id)
		{
			return Response.Empty();
		}

		// WORKS!
		public Response GetEmptyCreatedResponse(int id)
		{
			return Response.Created();
		}

		// WORKS!
		public Response GetEmptySuccessResponse(int id)
		{
			return Response.Success();
		}

		// WORKS!
		public Response<string> GetAsResponseT(int id)
		{
			var str = "12345";

			return Response<string>.Created(str);
		}


		public Response<string> GetAsResponseTError(int id)
		{
			var str = "123";

			if (id < 1)
			{
				return ResponseError.Invalid("!", id);
			}

			return Response<string>.Created(str);
		}


		// WORKS; ODD!
		public Response GetResponseFromAnotherResponseError(int id)
		{
			Response respErr = GetRespErr();

			if (respErr.IsError)
			{
				return Response.TransferError(respErr);
			}

			// odd that this works?!
			return Response<WeatherForecast>.Created(new());

			// works too (as expected)
			//return Response.Success();
		}

		private Response GetRespErr()
		{
			return ResponseError.Invalid("Invalid id!");
		}

		// WORKS!
		public Response GetErrorResponse()
		{
			try
			{
				throw new ArgumentException("missing!");
			}
			catch (ApplicationException ex)
			{
				return ex;
			}
			catch (Exception ex)
			{
				return ex;
			}
		}

		// WORKS!
		public async Task<Response<WeatherForecast>> GetCreatedResponse()
		{
			await Task.Delay(1);

			return Response<WeatherForecast>.Created(new ());

			// works too!
			//return new WeatherForecast();
		}


		// WORKS!
		public Response<WeatherForecast> GetWeatherBySummary(string summary)
		{

			Response<string> respErr = CheckSummary(summary);

			if (respErr.IsError)
			{
				//return Response.TransferError<WeatherForecast>(respErr);

				// shorthand!
				return respErr.To<WeatherForecast>();
			}

			return Response<WeatherForecast>.Success(new());
		}

		private static Response<string> CheckSummary(string summary)
		{
			if (string.IsNullOrWhiteSpace(summary))
			{
				return ResponseError.Invalid("Invalid summary!");
			}

			var isExistingSummary = Summaries.Any(x => x.Equals(summary, StringComparison.OrdinalIgnoreCase));

			if (isExistingSummary)
			{
				return ResponseError.NotFound("Summary was not found!");
			}

			return summary;
		}

		// WORKS!
		public Response<WeatherForecast> GetMatch(string summary)
		{
			Response<string> resp = CheckSummary(summary);

			return resp.Match(
				success => Response<WeatherForecast>.Success(new ()),
				error => resp.To<WeatherForecast>()
			);
		}
	}
}
