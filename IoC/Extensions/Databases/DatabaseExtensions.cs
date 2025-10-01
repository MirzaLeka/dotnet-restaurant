using DotNet8Starter.Helpers.Constants;
using Microsoft.EntityFrameworkCore;

namespace DotNet8Starter.IoC.Extensions.Databases
{
	public static class DatabaseExtensions
	{
		public static IServiceCollection AddDatabaseExtensions(
			this IServiceCollection services,
			IConfiguration configuration
		)
		{
			ArgumentNullException.ThrowIfNull(services);
			ArgumentNullException.ThrowIfNull(configuration);

			// Reverse ENgineering

			//services.AddDbContext<MyDbContext>(options =>
			//{
			//options.UseSqlServer(configuration.GetConnectionString(DB.APP_DB));

			//	options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
			//});

			return services;

		}
	}
}
