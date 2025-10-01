using System.Diagnostics;

namespace DotNet8Starter.IoC.Middleware
{
	public class TrackRequestDurationMiddleware(RequestDelegate next)
	{
		private readonly RequestDelegate _next = next;

		public async Task InvokeAsync(HttpContext context)
		{
			var currentTime = Stopwatch.StartNew();

			context.Response.OnStarting(() =>
			{
				var elapsedTime = currentTime.ElapsedMilliseconds;
				context.Response.Headers.Append("X-Reuqest-Duration", $"{elapsedTime} ms");
				return Task.CompletedTask;
			});

			await _next(context);
		}

	}
}
