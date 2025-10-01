using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace DotNet8Starter.IoC.Extensions.Logging
{
	public static class SerilogExtensions
	{
		public static IHostBuilder AddSerilogExtension(this IHostBuilder host)
		{
			ArgumentNullException.ThrowIfNull(host, nameof(host));

			// Replace main logger
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console(theme: AnsiConsoleTheme.Code)
				.CreateLogger();

			// Add serilog logger
			return host.UseSerilog(
				(context, options) => 
					options
						.ReadFrom.Configuration(context.Configuration)
						.Enrich.FromLogContext()
						.WriteTo.Console(theme: AnsiConsoleTheme.Code)
			);
		}
	}
}
