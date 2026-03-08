using DotNet8Starter.BL.ServiceInterfaces;

namespace DotNet8Starter.BL.Services
{
	public class AppLogger<T> : IAppLogger<T> where T : class
	{

		private readonly ILogger<T> _logger;

		public AppLogger(ILogger<T> logger)
		{
			_logger = logger;
		}

		public void LogHTTPError(Exception ex, HttpContext httpContext, string statusCode, string id)
		{
			_logger.LogError(ex, "{Path} {Method}, {id}, {statusCode}",
				 httpContext.Request.Path, 
				 httpContext.Request.Method,
				 id,
				 statusCode
				);
		}
	}
}
