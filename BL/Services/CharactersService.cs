using DotNet8Starter.BL.ServiceInterfaces;
using DotNet8Starter.DL.DTO;
using DotNet8Starter.DL.Models;
using DotNet8Starter.Exceptions.Validations;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace DotNet8Starter.BL.Services
{
	public class CharactersService(ILogger<CharactersService> logger, IHttpClientFactory httpClientFactory) : ICharactersService
	{
		private readonly JsonSerializerSettings _serializerSettings = new ()
		{
			ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
		};

		public async Task<Response<Character>> CreateNewCharacter(CharacterPayload payload)
		{
			try
			{
				var httpClient = httpClientFactory.CreateClient("CharactersAPI");

				string jsonString = JsonConvert.SerializeObject(payload, _serializerSettings);
				var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

				var characterResult = await httpClient.PostAsync("/api/Characters/New", content);

				characterResult.EnsureSuccessStatusCode();

				return await characterResult.Content.ReadAsAsync<Character>();
			}
			catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
			{
				// question: if I throw here will the global exception handler capture the HttpRequestException ?

				logger.LogError(ex, "Create new character exception!");

				switch (ex.StatusCode)
				{
					case HttpStatusCode.Unauthorized:
					case HttpStatusCode.Forbidden:
						return ResponseError.Unauthorized(ex);
					case HttpStatusCode.BadRequest:
						return ResponseError.BadRequest(ex);
					default:
						return ResponseError.InternalError(ex);
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Create new character exception!");
				return ResponseError.InternalError(ex);
			}
		}



		public async Task<Character> GetCharacterById(int id)
		{
			var httpClient = httpClientFactory.CreateClient("CharactersAPI");
			var characterResult = await httpClient.GetAsync($"/api/Characters/{id}");

			if (!characterResult.IsSuccessStatusCode)
			{
				return null;
			}

			return await characterResult.Content.ReadAsAsync<Character>();
		}

		public async Task<List<Character>> GetAllCharacters()
		{
			var httpClient = httpClientFactory.CreateClient("CharactersAPI");
			var characterResult = await httpClient.GetAsync("/api/Characters");

			return await characterResult.Content.ReadAsAsync<List<Character>>();
		}




		public async Task<Response<Character>> UpdateCharacter(int id, CharacterPayload payload)
		{
			try
			{
				var httpClient = httpClientFactory.CreateClient("CharactersAPI");

				string jsonString = JsonConvert.SerializeObject(payload, _serializerSettings);
				var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

				var response = await httpClient.PutAsync($"/api/Characters/Update/{id}/broken", content);

				// SUCCESS PATH
				if (response.IsSuccessStatusCode)
				{
					var character = await response.Content.ReadAsAsync<Character>();
					return Response<Character>.Success(character);
				}

				// ERROR PATH — capture headers + body
				//var errorBody = await response.Content.ReadAsStringAsync();
				//var headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value));
				//var contentHeaders = response.Content.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value));

				//logger.LogError(
				//	"UpdateCharacter failed. Status: {Status}. Headers: {@Headers}. ContentHeaders: {@ContentHeaders}. Body: {Body}",
				//	(int)response.StatusCode,
				//	headers,
				//	contentHeaders,
				//	errorBody
				//);

				var errorBody = await response.Content.ReadAsStringAsync();

				var logObject = new
				{
					StatusCode = (int)response.StatusCode,
					ReasonPhrase = response.ReasonPhrase,
					RequestUri = response.RequestMessage?.RequestUri?.ToString(),
					Method = response.RequestMessage?.Method?.ToString(),
					RequestHeaders = response.RequestMessage?.Headers
						.ToDictionary(h => h.Key, h => string.Join(",", h.Value)),
					ResponseHeaders = response.Headers
						.ToDictionary(h => h.Key, h => string.Join(",", h.Value)),
					ContentHeaders = response.Content.Headers
						.ToDictionary(h => h.Key, h => string.Join(",", h.Value)),
					Body = errorBody,
					Payload = jsonString,
					Timestamp = DateTime.UtcNow
				};

				logger.LogError("HTTP error response: {Response}", logObject);

				return MapError(response.StatusCode, errorBody);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Unexpected exception in UpdateCharacter");
				return ResponseError.InternalError(ex);
			}
		}

		private static Response<Character> MapError(HttpStatusCode status, string errorBody)
		{
			return status switch
			{
				HttpStatusCode.MethodNotAllowed => ResponseError.MethodNotAllowed(errorBody),
				HttpStatusCode.NotFound => ResponseError.NotFound(errorBody),
				HttpStatusCode.Unauthorized => ResponseError.Unauthorized(errorBody),
				HttpStatusCode.Forbidden => ResponseError.Unauthorized(errorBody),
				HttpStatusCode.BadRequest => ResponseError.BadRequest(errorBody),
				_ => ResponseError.InternalError(errorBody)
			};
		}

		//public async Task<Response<Character>> UpdateCharacter(int id, CharacterPayload payload)
		//{
		//	try
		//	{
		//		var httpClient = httpClientFactory.CreateClient("CharactersAPI");
		//		string jsonString = JsonConvert.SerializeObject(payload,_serializerSettings);
		//		var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

		//		var characterResult = await httpClient.PutAsync($"/api/Characters/Update/{id}/broken", content);

		//		characterResult.EnsureSuccessStatusCode();

		//		return await characterResult.Content.ReadAsAsync<Character>();
		//	}
		//	catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
		//	{
		//		logger.LogError(ex, "Create new character exception!");

		//		switch (ex.StatusCode)
		//		{
		//			case HttpStatusCode.MethodNotAllowed:
		//				return ResponseError.MethodNotAllowed(ex);
		//			case HttpStatusCode.NotFound:
		//				return ResponseError.NotFound(ex);
		//			case HttpStatusCode.Unauthorized:
		//			case HttpStatusCode.Forbidden:
		//				return ResponseError.Unauthorized(ex);
		//			case HttpStatusCode.BadRequest:
		//				return ResponseError.BadRequest(ex);
		//			default:
		//				return ResponseError.InternalError(ex);
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		logger.LogError(ex, "Create new character exception!");
		//		return ResponseError.InternalError(ex);
		//	}
		//}

		public Task<Character> DeleteCharacter(int id)
		{
			var httpClient = httpClientFactory.CreateClient("CharactersAPI");
			throw new NotImplementedException();
		}
	}
}
