using DotNet8Starter.BL.ServiceInterfaces;
using DotNet8Starter.BL.Services;
using DotNet8Starter.IoC.Extensions.Authentication;
using DotNet8Starter.IoC.Extensions.Controllers;
using DotNet8Starter.IoC.Extensions.Databases;
using DotNet8Starter.IoC.Extensions.ExceptionHandling;
using DotNet8Starter.IoC.Extensions.Health;
using DotNet8Starter.IoC.Extensions.HTTP;
using DotNet8Starter.IoC.Extensions.Logging;
using DotNet8Starter.IoC.Extensions.Swagger;

namespace DotNet8Starter.IoC
{
	public static class DepdenencyContainer
	{
		public static IServiceCollection AddServicesExtensions(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env, IHostBuilder host) 
		{
			#region Register builder services for Dependency Injection


			services.AddControllersExtension();

			// Global Exception Handler
			services.AddExceptionHandlingExtension(env);

			// Health Checks
			services.AddHealthCheckExtensions(configuration);

			// Documentation
			services.AddSwaggerExtension();

			// Databases
			services.AddDatabaseExtensions(configuration);

			// Add Authentication
			services.AddAuthenticationExtension(configuration);

			// HTTP Clients
			services.AddHttpExtensions(configuration);

			// Logging
			host.AddSerilogExtension();

			#endregion

			services.AddScoped<IOrcherstrationService, OrcherstrationService>();
			services.AddScoped<ILogOrcherstrationExceptionsService, LogOrcherstrationExceptionsService>();
			services.AddScoped<IOrderService, OrderService>();
			services.AddScoped<IReplicationService, ReplicationService>();


			services.AddSingleton<IAMQPublisherService, AMQPublisherService>();
			services.AddHostedService<WorkerService>();

			return services;
		}
	}
}
