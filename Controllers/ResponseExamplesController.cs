using DotNet8Starter.BL.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace DotNet8Starter.Controllers
{

	[Route("api/[controller]")]
	[ApiController]
	public class ResponseExamplesController(IResultExamplesService resultExamplesService) : ControllerBase
	{
		private readonly IResultExamplesService _resultExamplesService = resultExamplesService;

		[HttpGet("")]
		public ActionResult Get()
		{
			var resp = _resultExamplesService.CheckErrorObj();

			var b = resp.Status;

			if (resp.IsError)
			{
				return StatusCode(resp.RError.Status, new { id = 5001, resp.RError.Message });
			}

			return StatusCode((int)resp.Status, resp.Value);
		}
	}
}
