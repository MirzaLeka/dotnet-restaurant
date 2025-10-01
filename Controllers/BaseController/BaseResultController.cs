using DotNet8Starter.Exceptions.Validations;
using Microsoft.AspNetCore.Mvc;

namespace DotNet8Starter.Controllers.BaseController
{
	public class BaseResultController : ControllerBase
	{

		public ActionResult<T> HttpExceptionFromResult<T>(Result<T> result)
		{
			return result.IsValidationError ?
				BadRequestFromResult(result) :
					result.IsNotFoundError ?
				BNotFoundFromResult(result) :
					InternalErrorFromResult(result);
		}

		public ActionResult<T> BadRequestFromResult<T>(Result<T> result)
		{
			var problemDetails = ToProblemDetails(result, StatusCodes.Status400BadRequest, Request);
			return BadRequest(problemDetails);
		}

		public ActionResult<T> BNotFoundFromResult<T>(Result<T> result)
		{
			var problemDetails = ToProblemDetails(result, StatusCodes.Status404NotFound, Request);
			return NotFound(problemDetails);
		}

		public ActionResult<T> InternalErrorFromResult<T>(Result<T> result)
		{
			var problemDetails = ToProblemDetails(result, StatusCodes.Status500InternalServerError, Request);
			return StatusCode(400, problemDetails);
		}

		private static ProblemDetails ToProblemDetails<T> (Result<T> result, int statusCode, HttpRequest request)
		{
			return new ProblemDetails
			{
				Title = GetExceptionTitle(result.ExceptionType),
				Detail = result.ExceptionMessage,
				Status = statusCode,
				Instance = $"{request.Method} {request.Path}",
				Extensions = new Dictionary<string, object?>
				{
					["timestamp"] = DateTime.UtcNow
				}
			};
		}

		private static string GetExceptionTitle(ResultException ex)
		{
			return ex switch
			{
				ResultException.ValidationException => "Validation Exception",
				ResultException.NotFoundException => "Not Found Exception",
				_ => "Internal Server Exception"
			};
		}
	}
}
