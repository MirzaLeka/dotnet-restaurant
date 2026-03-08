using System.Runtime.CompilerServices;

namespace DotNet8Starter.Exceptions.Validations
{

	public sealed class ResultTask<T>
	{
		private readonly Task<Result<T>> _task;

		public ResultTask(Task<Result<T>> task)
		{
			_task = task ?? throw new ArgumentNullException(nameof(task));
		}

		public TaskAwaiter<Result<T>> GetAwaiter() => _task.GetAwaiter();

		public Task<Result<T>> AsTask() => _task;

		// Implicit conversions allow seamless use with async methods
		public static implicit operator Task<Result<T>>(ResultTask<T> resultTask) => resultTask._task;
		public static implicit operator ResultTask<T>(Task<Result<T>> task) => new ResultTask<T>(task);

		// Convenience factories
		public static ResultTask<T> FromResult(Result<T> result) => new ResultTask<T>(Task.FromResult(result));
		public static ResultTask<T> Success(T value) => FromResult(Result<T>.Success(value));
		public static ResultTask<T> ValidationError(string message) => FromResult(Result<T>.BadRequestError(message));
		public static ResultTask<T> NotFoundError(string message) => FromResult(Result<T>.NotFoundError(message));
		public static ResultTask<T> InternalServerError(string message) => FromResult(Result<T>.InternalServerError(message));
	}


	//public class ResultTask<T>
	//{
	//	private readonly Task<Result<T>> _task;

	//	private ResultTask(Task<Result<T>> task)
	//	{
	//		_task = task;
	//	}

	//	public TaskAwaiter<Result<T>> GetAwaiter() => _task.GetAwaiter();

	//	public Task<Result<T>> AsTask() => _task; // expose underlying Task

	//	public static ResultTask<T> FromResult(Result<T> result)
	//		=> new ResultTask<T>(Task.FromResult(result));

	//	public static ResultTask<T> Success(T value)
	//		=> FromResult(Result<T>.Success(value));

	//	public static ResultTask<T> ValidationError(string errorMessage)
	//		=> FromResult(Result<T>.ValidationError(errorMessage));

	//	public static ResultTask<T> NotFoundError(string errorMessage)
	//		=> FromResult(Result<T>.NotFoundError(errorMessage));

	//	public static ResultTask<T> InternalServerError(string errorMessage)
	//		=> FromResult(Result<T>.InternalServerError(errorMessage));
	//}
}
