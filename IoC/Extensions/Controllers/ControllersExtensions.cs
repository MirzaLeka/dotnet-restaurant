namespace DotNet8Starter.IoC.Extensions.Controllers
{
	public static class ControllersExtensions
	{
		public static IServiceCollection AddControllersExtension(this IServiceCollection services) 
		{
			services.AddControllers();
			services.AddEndpointsApiExplorer();

			return services;
		}
	}
}
