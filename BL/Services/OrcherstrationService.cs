using DotNet8Starter.BL.ServiceInterfaces;
using DotNet8Starter.DL.Models;
using DotNet8Starter.Helpers.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotNet8Starter.BL.Services
{
	public class OrcherstrationService(ILogger<OrcherstrationService> logService, IOrderService orderService, IAMQPublisherService amqService) : IOrcherstrationService
	{
		private readonly ILogger<OrcherstrationService> _logService = logService;
		private readonly IOrderService _orderService = orderService;
		private readonly IAMQPublisherService _amqService = amqService;


		public async Task OrcherstrateEvents(string message, string? correlationId)
		{
			try
			{
				var envelope = JsonConvert.DeserializeObject<OrderEvent>(message);

				if (envelope is null) return;

				if (envelope.Occurrences >= 3)
				{
					return;
				}

				envelope.Occurrences += 1;

				switch (envelope.EventName)
				{
					case EventName.MAKE_PIZZA:
						await HandleMakePizzaEvent(envelope);
						break;

					case EventName.MAKE_LEMONADE:
						await HandleMakeLemonadeEvent(envelope);
						break;

					case EventName.DELIVER_BILL:
						_orderService.DeliverBill();
						break;

					default:
						Console.WriteLine($"Unknown event: {envelope.EventName}");
						break;
				}
			}
			catch (Exception ex)
			{
				_logService.LogError(
					ex, "Unable to deserialize the event: {message}, {innerException}", ex.Message, ex.InnerException?.Message
				);
				return;
			}

		}

		private async Task HandleMakePizzaEvent(OrderEvent envelope)
		{
			var pizza = envelope.Payload.ToObject<MakePizza>();

			if (pizza is null) return;

			var createPizzaResponse = await _orderService.CreatePizza(pizza);

			if (!createPizzaResponse.IsSuccessful)
			{
				if (createPizzaResponse.IsEligibleForRetry)
				{
					_amqService.SendMessage(envelope.EventName, pizza, envelope.Occurrences);

				}
				else
				{
					Console.WriteLine("save into the table");
				}
				return;
			}

			_amqService.SendMessage(EventName.DELIVER_BILL, new {});

		}

		private async Task HandleMakeLemonadeEvent(OrderEvent envelope)
		{
			var lemonade = envelope.Payload.ToObject<MakeLemonade>();

			if (lemonade is null) return;

			var createLemonadeResponse = await _orderService.CreateLemonade(lemonade);

			if (!createLemonadeResponse.IsSuccessful)
			{
				if (createLemonadeResponse.IsEligibleForRetry)
				{
					//ExecuteEvent(envelope.EventName, lemonade, envelope.Occurrences);
					_amqService.SendMessage(envelope.EventName, lemonade, envelope.Occurrences);
				}
				else
				{
					Console.WriteLine("save into the table resp status code, error message, original payload");
				}

				return;
			}

			var makePizzaRuquest = MakePizza.ToMakePizza("Margherita", createLemonadeResponse.ResponseBody.SecretIngridient);

			var isSuccessful = _amqService.SendMessage(EventName.MAKE_PIZZA, makePizzaRuquest);

			if (!isSuccessful)
			{
				// log exception
				Console.WriteLine("reroute to table");
			}
		}

	}
}

