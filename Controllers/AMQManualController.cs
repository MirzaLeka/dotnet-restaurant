using DotNet8Starter.BL.ServiceInterfaces;
using DotNet8Starter.DL.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotNet8Starter.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AMQManualController(IOrderService orderService) : ControllerBase
	{
		private readonly IOrderService _orderService = orderService;

		[HttpPost("makeLemonade")]
		public async Task MakeLemonade([FromBody] MakeLemonade nade)
		{
			await _orderService.ContactRetaurant(nade);
		}

	}
}
