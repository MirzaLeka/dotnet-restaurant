using DotNet8Starter.DL.Models;

namespace DotNet8Starter.BL.ServiceInterfaces
{
	public interface IJsonPlaceholderService
	{
		Task<List<Todo>?> GetAllTodos();
		Task<Todo?> GetTodoByID(int todoId);
	}
}
