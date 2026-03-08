namespace DotNet8Starter.Exceptions.Validations
{
	public static class ResultExtensions
	{

		// todo side effect metoda za logove ili nesto
		// todo integrate with fluent => umjesto isValid da dobijes result sa greskom

		/// <summary>
		/// Overrides the value or the error message
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="result"></param>
		/// <param name="newValue"></param>
		/// <returns></returns>
		public static Result<T> Update<T>(this Result<T> result, T newValue)
		{

			if (result.IsError)
			{
				return Result<T>.From(default, newValue as string, result.ResultStatus);
			}

			return Result<T>.From(newValue, null, result.ResultStatus);
		}

		public static Result<string> Append(this Result<string> result, string newValue)
		{
			var appended = (result.Value ?? string.Empty) + newValue;
			return Result<string>.From(appended, result.ExceptionMessage, result.ResultStatus);
		}

		public static Result<List<T>> Add<T>(this Result<List<T>> result, T newValue)
		{
			var lista = result.Value;

			lista.Add(newValue);
			var newResult = lista;

			return Result<List<T>>.From(newResult, null, result.ResultStatus);
		}


		//public static Result<IEnumerable<T>> Add<T>(this Result<IEnumerable<T>> result, T newValue)
		//{
		//	result.Value.ToList().Add(newValue);
		//	return Result<IEnumerable<T>>.From(result.Value, null, result.ResultStatus);
		//}

		/// <summary>
		/// Update value/error message and status
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="result"></param>
		/// <param name="newValue"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static Result<T> Convert<T>(this Result<T> result, ResultStatus status, T newValue)
		{

			if (status != ResultStatus.Ok)
			{
				return Result<T>.From(default, newValue as string, status);
			}

			return Result<T>.From(newValue, null, status);
		}

		/// <summary>
		/// Convert but preserve error message
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="result"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static Result<T> Convert<T>(this Result<T> result, ResultStatus status)
		{

			if (status != ResultStatus.Ok)
			{
				if (result.Value is string)
				{
					return Result<T>.From(default, result.Value as string, status);
				}

				return Result<T>.From(default, result.ExceptionMessage, status);
			}

			return Result<T>.From(result.Value, null, status);
		}

		public static Result<T> Clear<T>(this Result<T> result)
		{
			if (result.IsError)
			{
				return Result<T>.From(default, null, ResultStatus.InternalServerException);
			}

			// todo implement Default()
			return Result<T>.From(default, null, ResultStatus.Ok);
		}

		/// <summary>
		/// Na fabricke postavke
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="result"></param>
		/// <returns></returns>
		public static Result<T> Reset<T>(this Result<T> result)
		{
			// todo implement Default()
			return Result<T>.From(default, null, ResultStatus.None);
		}

	}
}
