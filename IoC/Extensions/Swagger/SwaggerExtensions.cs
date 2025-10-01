namespace DotNet8Starter.IoC.Extensions.Swagger
{
	public static class SwaggerExtensions
	{
		public static IServiceCollection AddSwaggerExtension(this IServiceCollection services)
		{
			services.AddSwaggerGen();

			return services;
		}

		public static void UseSwaggerExtension(this WebApplication app)
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}
	}
}
