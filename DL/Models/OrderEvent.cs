using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace DotNet8Starter.DL.Models
{
	public class OrderEvent
	{
		/// <summary>
		/// Routing Key
		/// </summary>
		[JsonProperty("eventName")]
		public string EventName { get; set; }

		/// <summary>
		/// Dynamic Payload
		/// </summary>
		[JsonProperty("payload")]
		public JObject Payload { get; set; }

		/// <summary>
		/// Retry count
		/// </summary>
		[JsonProperty("occurrences")]
		public int Occurrences { get; set; } = 0;

		public static string ToOrder(string eventName, object? payload, int occurrence = 1)
		{
			var order = new OrderEvent()
			{
				EventName = eventName,
				Payload = payload != null
				? JObject.FromObject(payload)
				: new JObject(),
				Occurrences = occurrence
			};

			return JsonConvert.SerializeObject(order);
		}
	}
}
