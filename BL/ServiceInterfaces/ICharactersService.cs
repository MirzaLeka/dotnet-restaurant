using DotNet8Starter.DL.DTO;
using DotNet8Starter.DL.Models;
using DotNet8Starter.Exceptions.Validations;

namespace DotNet8Starter.BL.ServiceInterfaces
{
	public interface ICharactersService
	{
		Task<List<Character>> GetAllCharacters();

		Task<Character> GetCharacterById(int id);

		Task<Response<Character>> CreateNewCharacter(CharacterPayload payload);

		Task<Response<Character>> UpdateCharacter(int id, CharacterPayload character);

		Task<Character> DeleteCharacter(int id);

	}
}
