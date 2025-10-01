using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DotNet8Starter.IoC.Extensions.Authentication
{
	public static class AuthenticationExtensions
	{
		public static IServiceCollection AddAuthenticationExtension(this IServiceCollection services, IConfiguration configuration) 
		{
			var jwtSecret = configuration.GetSection("JWTSecret").Get<string>();

			// Auth Middleware
			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = false,
					ValidateAudience = false,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					//ValidIssuer = "your-issuer",
					//ValidAudience = "your-audience",
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
				};
			});

			return services;
		}
	}
}
