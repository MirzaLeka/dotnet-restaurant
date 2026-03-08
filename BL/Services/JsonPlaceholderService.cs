using DotNet8Starter.BL.ServiceInterfaces;
using DotNet8Starter.DL.Models;
using Newtonsoft.Json;

namespace DotNet8Starter.BL.Services
{
	public class JsonPlaceholderService(HttpClient client) : IJsonPlaceholderService
	{
		// IHttpClientFactory httpClientFactory
		//private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
		private readonly HttpClient _client = client;


		public async Task<List<Todo>?> GetAllTodos()
		{
			//var client = _httpClientFactory.CreateClient("JsonPlaceholderAPI");
			var httpResponse = await _client.GetAsync("/Todos");

			if (httpResponse.IsSuccessStatusCode)
			{
				var responseString = await httpResponse.Content.ReadAsStringAsync();
				var todos = JsonConvert.DeserializeObject<List<Todo>>(responseString);
				return todos;
			}
			return [];
		}

		public async Task<Todo> GetTodoByID(int todoId)
		{
			return new Todo();
		}
	}
}
