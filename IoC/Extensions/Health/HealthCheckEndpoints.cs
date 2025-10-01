using DotNet8Starter.Helpers.Constants;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace DotNet8Starter.IoC.Extensions.Health
{
	public static class HealthCheckEndpoints
	{
		public static IEndpointRouteBuilder MapCustomHealthChecks(this IEndpointRouteBuilder builder) 
		{
			builder.MapHealthChecks("/health/live", new HealthCheckOptions
			{
				Predicate = check => check.Name == "self",
				ResponseWriter = WriteToHealthResponse,
				ResultStatusCodes = _HealthStatuses
			});

			builder.MapHealthChecks("/health/ready", new HealthCheckOptions
			{
				Predicate = check => check.Tags.Contains("ready"), // only tagged items
				ResponseWriter = WriteToHealthResponse,
				ResultStatusCodes = _HealthStatuses
			});

			return builder;
		}

		private static readonly IDictionary<HealthStatus, int> _HealthStatuses = new Dictionary<HealthStatus, int>
		{
			{ HealthStatus.Healthy, StatusCodes.Status200OK },
			{ HealthStatus.Degraded, StatusCodes.Status200OK },
			{ HealthStatus.Unhealthy, StatusCodes.Status503ServiceUnavailable },
		};

		private static Task WriteToHealthResponse(HttpContext context, HealthReport report)
		{
			context.Response.ContentType = ContentTypes.APPLICATION_JSON;

			var result = JsonSerializer.Serialize(new
			{
				status = report.Status.ToString(),
				checks = report.Entries.Select(e => new
				{
					name = e.Key,
					status = e.Value.Status.ToString(),
					description = e.Value.Description,
				}),
				duration = report.TotalDuration
			});

			return context.Response.WriteAsync(result);
		}
	}
}
