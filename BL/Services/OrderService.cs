using DotNet8Starter.BL.ServiceInterfaces;
using DotNet8Starter.DL.DTO;
using DotNet8Starter.DL.Models;
using DotNet8Starter.Helpers.Constants;
using System;

namespace DotNet8Starter.BL.Services
{
	public class OrderService(IHttpClientFactory httpFactory, IAMQPublisherService amqService) : IOrderService
	{
		private readonly IHttpClientFactory _httpClientFactory = httpFactory;
		private readonly IAMQPublisherService _amqService = amqService;

		public async Task<ExternalAPIResponse<MakeLemonadeResponse>> CreateLemonade(MakeLemonade lemonade)
		{
			var httpClient = _httpClientFactory.CreateClient("NodejsAPI");
			var result = await httpClient.PostAsJsonAsync($"api/drinks/make-lemonade", lemonade);

			if (result.IsSuccessStatusCode)
			{
				var lemonadeResponse = await result.Content.ReadAsAsync<MakeLemonadeResponse>();
				var response = ExternalAPIResponse<MakeLemonadeResponse>.ToSuccessfulResponse(lemonadeResponse);

				return response;
			}

			return ExternalAPIResponse<MakeLemonadeResponse>.ToFailedResponse(true, "error", result.StatusCode);
		}


		public async Task<ExternalAPIResponse<CreatePizzaResponseDTO>> CreatePizza(MakePizza pizza)
		{
			var httpClient = _httpClientFactory.CreateClient("NodejsAPI");
			var response = await httpClient.PostAsJsonAsync($"api/food/make-pizza", pizza);

			var respContent = await response.Content.ReadAsAsync<CreatePizzaResponseDTO>();
			return ExternalAPIResponse<CreatePizzaResponseDTO>.ToSuccessfulResponse(respContent);
		}


		public void DeliverBill()
		{
			Console.WriteLine("Done!");
		}

		public async Task ContactRetaurant(MakeLemonade lemonade)
		{
			_amqService.SendMessage(EventName.MAKE_LEMONADE, lemonade);
		}

		public async Task<double> GetKelvinTemperature(int temperatureC)
		{
			var httpClient = _httpClientFactory.CreateClient("NodejsAPI");
			var result = await httpClient.GetAsync($"api/getKelvinTemperature/{temperatureC}");

			if (result.IsSuccessStatusCode)
			{
				return await result.Content.ReadAsAsync<double>();
			}

			return 0;
		}
	}
}
