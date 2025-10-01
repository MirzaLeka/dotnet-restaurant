using DotNet8Starter.Helpers.Constants;

namespace DotNet8Starter.IoC.Extensions.Health
{
	public static class HealthCheckExtensions
	{
		public static IServiceCollection AddHealthCheckExtensions(this IServiceCollection services, IConfiguration configuration) 
		{
			ArgumentNullException.ThrowIfNull(services, nameof(services));
			ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

			services.AddSelfHealtCheck();
			//services.AddDependencyHealtCheck(configuration, DB.APP_DB);

			return services;
		}
	}
}
