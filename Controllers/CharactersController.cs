using DotNet8Starter.BL.ServiceInterfaces;
using DotNet8Starter.DL.DTO;
using DotNet8Starter.DL.Models;
using Microsoft.AspNetCore.Mvc;

namespace DotNet8Starter.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CharactersController(ICharactersService charactersService) : ControllerBase
	{
		[HttpGet("")]
		public async Task<IActionResult> GetAllCharacters()
		{
			var response = await charactersService.GetAllCharacters();
			return Ok(response);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetCharacterById([FromRoute] int id)
		{
			var response = await charactersService.GetCharacterById(id);
			return Ok(response);
		}

		[HttpPost("New")]
		public async Task<ActionResult<Character>> CreateCharacter([FromBody] CharacterPayload payload)
		{
			var response = await charactersService.CreateNewCharacter(payload);

			return response.IsSuccessful ?
				Ok(response.Value) :
				StatusCode(response.RError.Status, response.RError);
		}

		[HttpPut("Update/{id}")]
		public async Task<ActionResult<Character>> UpdateCharacter([FromRoute] int id, [FromBody] CharacterPayload payload)
		{
			var response = await charactersService.UpdateCharacter(id, payload);

			return response.IsSuccessful ?
				Ok(response.Value) :
				StatusCode(response.RError.Status, response.RError);
		}
	}
}
