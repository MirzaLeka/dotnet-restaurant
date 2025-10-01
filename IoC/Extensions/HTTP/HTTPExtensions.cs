using DotNet8Starter.Helpers.Constants;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNet8Starter.IoC.Extensions.HTTP
{
	public static class HTTPExtensions
	{
		public static IServiceCollection AddHttpExtensions(this IServiceCollection services, IConfiguration configuration)
		{
			ArgumentNullException.ThrowIfNull(services, nameof(services));
			ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

			services.AddHttpContextAccessor();

			services.AddCustomHttpClient("NodejsAPI", configuration["NodejsAPIUri"]);

			return services;

		}

		private static void AddCustomHttpClient(this IServiceCollection services, string clientName, string clientUri)
		{
			var httpClientBuilder = services.AddHttpClient(clientName, c =>
			{
				c.BaseAddress = new Uri(clientUri?.ToString());
				c.DefaultRequestHeaders.Add("Accept", ContentTypes.APPLICATION_JSON);
			});
		}
	}
}
