using Microsoft.AspNetCore.Diagnostics;

namespace DotNet8Starter.IoC.Extensions.ExceptionHandling
{
	public static class ProblemDetailsConfig
	{
		public static IServiceCollection AddCustomProblemDetails(this IServiceCollection services, IWebHostEnvironment env)
		{
			services.AddProblemDetails(options =>
				options.CustomizeProblemDetails = context =>
				{
					SetInstance(context);
					//SetStatusCode(context);
					AddCustomExtensions(context, env);
				});

			return services;
		}

		private static void SetInstance(ProblemDetailsContext context) 
		{
			context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
		}

		//private static void SetStatusCode(ProblemDetailsContext context)
		//{
		//	context.ProblemDetails.Status = context.HttpContext.Response.StatusCode;
		//}

		private static void AddCustomExtensions(ProblemDetailsContext context, IWebHostEnvironment env)
		{
			var exception = context.HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

			SetTitle(context, exception);

			if (exception != null) 
			{
				context.ProblemDetails.Extensions.TryAdd("message", exception.Message);
				context.ProblemDetails.Extensions.TryAdd("innerException", exception.InnerException?.ToString());

				if (env.IsDevelopment())
				{
					context.ProblemDetails.Extensions.TryAdd("stackTrace", exception.StackTrace);
				}
			}

			context.ProblemDetails.Extensions.TryAdd("timestamp", DateTime.UtcNow);
		}

		private static void SetTitle(ProblemDetailsContext context, Exception? exception)
		{
			context.ProblemDetails.Type = exception != null ?
				exception.GetType().Name :
				"An unknown error occured!";
		}
	}
}
