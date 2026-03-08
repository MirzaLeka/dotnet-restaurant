using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DotNet8Starter.IoC.Middleware
{
	public class CharactersAuthHandler(IConfiguration config) : DelegatingHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			var token = config["CharactersApiToken"];

			if (!string.IsNullOrWhiteSpace(token))
			{
				// Add the custom header expected by Express
				request.Headers.Add("x-auth-token", token);
			}

			return base.SendAsync(request, cancellationToken);
		}
	}



}
