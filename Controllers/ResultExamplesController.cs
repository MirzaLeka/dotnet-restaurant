using DotNet8Starter.BL.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace DotNet8Starter.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ResultExamplesController(IResultExamplesService resultExamplesService) : ControllerBase
	{
		private readonly IResultExamplesService _resultExamplesService = resultExamplesService;

		[HttpGet("")]
		public ActionResult Get()
		{
			_resultExamplesService.GetEmpty();
			return Ok("Yeah!");
		}
	}
}
