using Microsoft.AspNetCore.Diagnostics;

namespace DotNet8Starter.Exceptions
{
	public sealed class GlobalExceptionHandler(
		IProblemDetailsService problemDetailsService,
		ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
	{

		public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
		{
			int statusCode = GetStatusCode(exception);

			// logger

			return false;
		}

		private int GetStatusCode(Exception exception) 
		{
			return exception switch
			{
				ApplicationException => StatusCodes.Status400BadRequest,
					_ => StatusCodes.Status500InternalServerError
			};
		}
	}
}

// c
// C