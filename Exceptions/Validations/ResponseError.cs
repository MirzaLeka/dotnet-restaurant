using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DotNet8Starter.Exceptions.Validations
{

	// Resume conversation
	// https://copilot.microsoft.com/shares/1goAZZJ259scXHGiG8CND


	// TODO exception name class with const string properties
	// Github aCtions that demonstrate CI progress
	// Messages from Express api vantage => Rosourxe not found! (404)

	// kad zoves Result.Validation() napravi se fluent validation. Prima ili "message" ili List<errors>

	// Fluent validation rules
	//public class ValidationFailure
	//{
	//	public string PropertyName { get; }
	//	public string ErrorMessage { get; }
	//	public string ErrorCode { get; }
	//	public object AttemptedValue { get; }
	//	public Severity Severity { get; }
	//	public Dictionary<string, object> CustomState { get; }
	//}

	//{
	//  "errors": {
	//    "WeatherType": [
	//	  "Weather type is required"
	//	]
	//},
	//  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
	//  "title": "One or more validation errors occurred.",
	//  "status": 400,
	//  "traceId": "..."
	//}



	public record RError
	{
		public HttpStatusCode StatusCode { get; private set; }
		public int Status { get; private set; }

		public string Message { get; private set; }
		public string? StackTrace { get; private set; }

		public string TraceId { get; private set; } = Guid.NewGuid().ToString();
		public object? Payload { get; private set; }

		// include traceId

		public RError(HttpStatusCode statusCode, string message)
		{
			StatusCode = statusCode;
			Status = (int)statusCode;
			Message = message;
		}

		// alternative

		public RError(int statusCode, string message)
		{
			// if status isn't valid => 500

			StatusCode = (HttpStatusCode)statusCode;
			Status = statusCode;
			Message = message;
		}

		public RError(HttpStatusCode statusCode, string message, string stackTrace)
		{
			StatusCode = statusCode;
			Status = (int)statusCode;
			Message = message;
			StackTrace = stackTrace;
		}

		// alternative

		public RError(int statusCode, string message, string stackTrace)
		{
			// if status isn't valid => 500

			StatusCode = (HttpStatusCode)statusCode;
			Status = statusCode;
			Message = message;
			StackTrace = stackTrace;
		}

		public RError(object payload)
		{
			StatusCode = HttpStatusCode.InternalServerError;
			Status = 500;
			Message = "Something went wrong!";
			StackTrace = string.Empty;
			Payload = payload;
		}

		public RError(HttpStatusCode statusCode, string message, object payload)
		{
			StatusCode = statusCode;
			Status = (int)statusCode;
			Message = message;
			StackTrace = string.Empty;
			Payload = payload;
		}

		public RError(HttpStatusCode statusCode, string message, string stackTrace, object payload)
		{
			StatusCode = statusCode;
			Status = (int)statusCode;
			Message = message;
			StackTrace = stackTrace;
			Payload = payload;
		}

		// RError As Problem
		public RError(ProblemDetails problemDetails)
		{
			// Payload = problemDetails

			Payload = new ProblemDetails
			{
				Title = "Invalid weather type",
				Detail = "weatherType cannot be null",
				Status = StatusCodes.Status400BadRequest,
				Extensions = { ["traceId"] = Guid.NewGuid().ToString() }

			//context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
			};
		}

		/// <summary>
		/// Substitute for nullable RError
		/// </summary>
		/// <returns>RError without error.</returns>
		public static RError NoError()
		{
			return new RError(HttpStatusCode.OK, string.Empty, string.Empty);
		}

		/// <summary>
		/// Whenever you call Response.Error() without being explicit
		/// </summary>
		/// <returns></returns>
		public static RError NewRError()
		{
			return new RError(HttpStatusCode.InternalServerError, "Internal Server Error!", string.Empty);
		}

		/// <summary>
		/// Whenever you call Response.Error() without being completely explicit
		/// </summary>
		/// <returns></returns>
		public static RError NewRError(string message)
		{
			return new RError(HttpStatusCode.InternalServerError, message, string.Empty);
		}

		/// <summary>
		/// HttpError built with Response.Error
		/// </summary>
		/// <param name="statusCode"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public static RError HttpError(int statusCode, string message)
		{
			// TODO if statusCode < 400, make it 500

			// TODO convert to HttpStatusCode.
			// If not status code, say it's 500
			return new RError((HttpStatusCode)statusCode, message, string.Empty);
		}

		public static RError HttpError(HttpStatusCode statusCode, string message)
		{
			// TODO if statusCode < 400, make it 500

			// TODO convert to HttpStatusCode.
			// If not status code, say it's 500
			return new RError(statusCode, message, string.Empty);
		}

		// TODO more variants for stack trace

	}
	/* , object MetaData = null */

	public class ResponseError
	{
		#region 400 - 499 status codes

		// 400 //

		public static RError Invalid(string message)
		{
			return new RError(HttpStatusCode.BadRequest, message);
		}

		public static RError Invalid(Exception ex)
		{
			return new RError(HttpStatusCode.BadRequest, ex.Message);
		}

		public static RError Invalid(string message, object invalidValue)
		{
			message += $", <{nameof(invalidValue)}>";
			return new RError(HttpStatusCode.BadRequest, message);
		}

		///
		/// alternative
		/// 

		public static RError BadRequest(string message)
		{
			return new RError(HttpStatusCode.BadRequest, message);
		}

		public static RError BadRequest(Exception ex)
		{
			return new RError(HttpStatusCode.BadRequest, ex.Message);
		}

		public static RError BadRequest(string message, object invalidValue)
		{
			message += $", <{nameof(invalidValue)}>";
			return new RError(HttpStatusCode.BadRequest, message);
		}

		// 401 //

		public static RError Unauthorized(string message)
		{
			return new RError(HttpStatusCode.Unauthorized, message);
		}

		public static RError Unauthorized(Exception ex)
		{
			return new RError(HttpStatusCode.Unauthorized, ex.Message);
		}

		// 403 //

		public static RError Forbidden(string message)
		{
			return new RError(HttpStatusCode.Forbidden, message);
		}

		public static RError Forbidden(Exception ex)
		{
			return new RError(HttpStatusCode.Forbidden, ex.Message);
		}

		// 404 //

		public static RError NotFound(string message)
		{
			return new RError(HttpStatusCode.NotFound, message);
		}

		public static RError NotFound(Exception ex)
		{
			return new RError(HttpStatusCode.NotFound, ex.Message);
		}

		// 405 //

		public static RError MethodNotAllowed(string message)
		{
			return new RError(HttpStatusCode.MethodNotAllowed, message);
		}

		public static RError MethodNotAllowed(Exception ex)
		{
			return new RError(HttpStatusCode.MethodNotAllowed, ex.Message);
		}

		// 409 //

		public static RError Conflict(string message)
		{
			return new RError(HttpStatusCode.Conflict, message);
		}

		public static RError Conflict(Exception ex)
		{
			return new RError(HttpStatusCode.Conflict, ex.Message);
		}

		// 422 //

		public static RError UnprocessableEntity(string message)
		{
			return new RError(HttpStatusCode.UnprocessableEntity, message);
		}

		public static RError UnprocessableEntity(Exception ex)
		{
			return new RError(HttpStatusCode.UnprocessableEntity, ex.Message);
		}

		#endregion

		#region 500 - 599 status codes

		// 500 //

		public static RError InternalError(string message)
		{
			return new RError(HttpStatusCode.InternalServerError, message);
		}

		public static RError InternalError(Exception ex)
		{
			return new RError(HttpStatusCode.InternalServerError, ex.Message);
		}

		// 502 //

		public static RError BadGateway(string message)
		{
			return new RError(HttpStatusCode.BadGateway, message);
		}

		public static RError BadGateway(Exception ex)
		{
			return new RError(HttpStatusCode.BadGateway, ex.Message);
		}

		// 503 //

		public static RError Unavailable(string message)
		{
			return new RError(HttpStatusCode.ServiceUnavailable, message);
		}

		public static RError Unavailable(Exception ex)
		{
			return new RError(HttpStatusCode.ServiceUnavailable, ex.Message);
		}

		#endregion
	}
}
