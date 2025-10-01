using DotNet8Starter.DL.DTO;
using DotNet8Starter.DL.Models;

namespace DotNet8Starter.BL.ServiceInterfaces
{
	public interface IOrderService
	{
		Task<double> GetKelvinTemperature(int temperatureC);

		public Task ContactRetaurant(MakeLemonade lemonade);

		Task<ExternalAPIResponse<CreatePizzaResponseDTO>> CreatePizza(MakePizza pizza);

		Task<ExternalAPIResponse<MakeLemonadeResponse>> CreateLemonade(MakeLemonade lemonade);

		void DeliverBill();

	}
}
