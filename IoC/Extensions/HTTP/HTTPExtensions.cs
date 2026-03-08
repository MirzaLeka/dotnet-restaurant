using DotNet8Starter.BL.ServiceInterfaces;
using DotNet8Starter.BL.Services;
using DotNet8Starter.Helpers.Constants;
using DotNet8Starter.IoC.Middleware;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Polly.Extensions.Http;
using System.Net;
using System.Net.Http.Headers;

namespace DotNet8Starter.IoC.Extensions.HTTP
{
	public static class HTTPExtensions
	{
		public static IServiceCollection AddHttpExtensions(this IServiceCollection services, IConfiguration configuration)
		{
			ArgumentNullException.ThrowIfNull(services, nameof(services));
			ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

			services.AddHttpContextAccessor();

			// named client
			var httpClientBuilder = services.AddHttpClient("CharactersAPI", client =>
			{
				client.BaseAddress = new Uri(configuration["CharactersUri"]?.ToString());
				client.DefaultRequestHeaders.Add("Accept", "application/json");

				//// Add your auth header here
				//client.DefaultRequestHeaders.Authorization =
				//	new AuthenticationHeaderValue("Bearer", configuration["CharactersApiToken"]);

			})
			.AddHttpMessageHandler<CharactersAuthHandler>()
			.AddPolicyHandler(GetRetryPolicy());


			// Typed HttpClient registration
			services.AddHttpClient<IJsonPlaceholderService, JsonPlaceholderService>(client =>
			{
				var uri = configuration["JsonPlaceholderUri"];
				client.BaseAddress = new Uri(uri);
				client.DefaultRequestHeaders.Add("Accept", "application/json");
			});

			// Testing typed clients
			// https://copilot.microsoft.com/shares/CYcYEmw7owoBPLc4hFGV7


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

		private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
		{
			return Policy<HttpResponseMessage>
				.HandleResult(response =>
				{
					// Retry ONLY on 405 or 500+
					bool isRetryStatus =
						response.StatusCode == HttpStatusCode.MethodNotAllowed ||
						(int)response.StatusCode >= 500;

					if (!isRetryStatus)
						return false;

					// Retry ONLY on the specific route
					var path = response.RequestMessage?.RequestUri?.AbsolutePath ?? "";
					return path.Contains("/api/Characters/Update") &&
						   path.Contains("/broken");
				})
				.WaitAndRetryAsync(
					retryCount: 10,
					sleepDurationProvider: attempt => TimeSpan.FromSeconds(1),
					onRetry: (outcome, timespan, attempt, context) =>
					{
						Console.WriteLine(
							$"Retry {attempt} after {timespan.TotalSeconds}s due to {outcome.Result.StatusCode}"
						);
					}
				);
		}

		//private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
		//{
		//	return HttpPolicyExtensions
		//		.HandleTransientHttpError() // 5xx, 408, network failures
		//		.OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // 429
		//		.WaitAndRetryAsync(
		//			retryCount: 3,
		//			sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
		//			onRetry: (outcome, timespan, attempt, context) =>
		//			{
		//				Console.WriteLine($"Retry {attempt} after {timespan.TotalSeconds}s due to {outcome.Result?.StatusCode}");
		//			});
		//}
	}
}
