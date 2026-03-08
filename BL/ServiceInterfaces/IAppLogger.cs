namespace DotNet8Starter.BL.ServiceInterfaces
{
	public interface IAppLogger<in T> where T : class
	{
		void LogHTTPError(Exception ex, HttpContext httpContext, string statusCode, string correlationId);
	}
}
