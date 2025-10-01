using DotNet8Starter.Exceptions;

namespace DotNet8Starter.IoC.Extensions.ExceptionHandling
{
	public static class ExceptionHandlingExtensions
	{
		public static IServiceCollection AddExceptionHandlingExtension(this IServiceCollection services, IWebHostEnvironment env) 
		{
			ArgumentNullException.ThrowIfNull(services, nameof(services));
			ArgumentNullException.ThrowIfNull(env, nameof(env));

			services.AddExceptionHandler<GlobalExceptionHandler>();
			services.AddCustomProblemDetails(env);

			return services;
		}
	}
}
