using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotNet8Starter.IoC.Extensions.Health
{
	public static class HealthCheckServices
	{
		public static IServiceCollection AddSelfHealtCheck(this IServiceCollection services)
		{
			// Liveness test
			services.AddHealthChecks()
				.AddCheck("self", () => HealthCheckResult.Healthy("App is alive!"));

			return services;
		}

		public static IServiceCollection AddDependencyHealtCheck(this IServiceCollection services, IConfiguration configuration, string dbName)
		{
			var connectionString = configuration.GetConnectionString(dbName);

			// Readyness test
			services.AddHealthChecks()
				.AddSqlServer(
					connectionString,
					name: dbName,
					failureStatus: HealthStatus.Unhealthy,
					tags: new[] { "ready" }
				);

			return services;
		}

	}
}
