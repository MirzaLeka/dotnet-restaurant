namespace DotNet8Starter.Exceptions.Validations
{
	public enum ResultException
	{
		None,
		ValidationException,
		NotFoundException,
		InternalServerException
	}

	public class Result<T>
	{
		internal T Value { get; private set; }
		internal string ExceptionMessage { get; private set; }
		internal ResultException ExceptionType { get; private set; }

		private Result(T value, string exceptionMessage, ResultException exceptionType)
		{
			Value = value;
			ExceptionMessage = exceptionMessage;
			ExceptionType = exceptionType;
		}

		public static Result<T> Success (T value)
		{
			return new Result<T>(value, null, ResultException.None);
		}

		public static Result<T> ValidationError(string errorMessage)
		{
			return new Result<T>(default, errorMessage, ResultException.ValidationException);
		}

		public static Result<T> NotFoundError(string errorMessage)
		{
			return new Result<T>(default, errorMessage, ResultException.NotFoundException);
		}

		public static Result<T> InternalServerError(string errorMessage)
		{
			return new Result<T>(default, errorMessage, ResultException.InternalServerException);
		}


		public bool IsSuccessful => ExceptionType == ResultException.None;
		public bool IsValidationError => ExceptionType == ResultException.ValidationException;
		public bool IsNotFoundError => ExceptionType == ResultException.NotFoundException;
		public bool IsInternalServerError => ExceptionType == ResultException.InternalServerException;

	}
}
