using DotNet8Starter.BL.ServiceInterfaces;
using DotNet8Starter.DL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotNet8Starter.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TodosController(IJsonPlaceholderService jsonPlaceholderService, IHttpClientFactory httpClientFactory) : ControllerBase
	{
		private readonly IJsonPlaceholderService _jsonPlaceholderService = jsonPlaceholderService;
		private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;


		[HttpGet("")]
		public async Task<ActionResult<List<Todo>?>> GetAllTodos()
		{
			var client = _httpClientFactory.CreateClient("JsonPlaceholderAPI");

			var res = await client.GetAsync("/Todos");

			return await _jsonPlaceholderService.GetAllTodos();
		}
	}
}
