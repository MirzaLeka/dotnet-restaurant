using DotNet8Starter.BL.ServiceInterfaces;
using Microsoft.AspNetCore.Diagnostics;

namespace DotNet8Starter.Exceptions
{
	public sealed class GlobalExceptionHandler(
		IProblemDetailsService problemDetailsService,
		IAppLogger<GlobalExceptionHandler> logger
		) : IExceptionHandler
	{

		private readonly IAppLogger<GlobalExceptionHandler> _logger = logger;

		public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
		{
			int statusCode = GetStatusCode(exception);

			_logger.LogHTTPError(exception, httpContext, statusCode.ToString(), "X-ID");

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