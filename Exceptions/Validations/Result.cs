namespace DotNet8Starter.Exceptions.Validations
{

	public class Result<T>
	{
		internal T Value { get; private set; }
		internal string ExceptionMessage { get; private set; }
		internal ResultStatus ResultStatus { get; private set; }

		private Result(T value, string exceptionMessage, ResultStatus resultStatus)
		{
			Value = value;
			ExceptionMessage = exceptionMessage;
			ResultStatus = resultStatus;
		}

		private Result(ResultStatus resultStatus)
		{
			ResultStatus = resultStatus;
		}

		private Result()
		{ }

		#region For Extensions

		// 👇 New internal factory
		internal static Result<T> From(T value, string message, ResultStatus status) =>
			new Result<T>(value, message, status);

		#endregion

		#region Simple extensions

		public static Result<T> New()
		{
			return new Result<T>();
		}

		public static Result<T> Success (T value)
		{
			return new Result<T>(value, null, ResultStatus.Ok);
		}


		// 👇 Implicit conversion from T to Result<T>
		public static implicit operator Result<T>(T value) =>
			Success(value);

		// Optional: implicit conversion back from Result<T> to T
		public static implicit operator T(Result<T> result) =>
			result.Value;


		public static Result<T> Error(string errorMessage)
		{
			return new Result<T>(default, errorMessage, ResultStatus.InternalServerException);
		}

		//public static implicit operator Result<T>(string errorMessage) =>
		//	Error(errorMessage);

		// Optional: implicit conversion back from Result<T> to T
		public static explicit operator string(Result<T> result) =>
			result.ExceptionMessage;

		#endregion

		#region HTTP status based extensions


		public static Result<T> Empty()
		{
			return new Result<T>(ResultStatus.Empty);
			//return new Result<T>(default, null, ResultStatus.Empty);

		}

		public static Result<T> BadRequestError(string errorMessage)
		{
			return new Result<T>(default, errorMessage, ResultStatus.BadRequestException);
		}

		public static Result<T> NotFoundError(string errorMessage)
		{
			return new Result<T>(default, errorMessage, ResultStatus.NotFoundException);
		}

		public static Result<T> InternalServerError(string errorMessage)
		{
			return new Result<T>(default, errorMessage, ResultStatus.InternalServerException);
		}

		#endregion

		#region Validation checks

		// napisi angular style getter za sve 3 varijante
		public bool IsSuccessful =>
			ResultStatus == ResultStatus.Ok || 
			ResultStatus == ResultStatus.Created ||
			ResultStatus == ResultStatus.Empty ||
			ResultStatus == ResultStatus.None;

		public bool IsError => !IsSuccessful;

		#endregion

		#region HTTP status specific validation checks

		public bool IsValidationError => ResultStatus == ResultStatus.BadRequestException;
		
		public bool IsUnauthorizedError => ResultStatus == ResultStatus.UnauthorizedException;
		//public bool IsInternalServerError => ResultStatus == ResultStatus.InternalServerException;
		//public bool IsInternalServerError => ResultStatus == ResultStatus.InternalServerException;
		//public bool IsInternalServerError => ResultStatus == ResultStatus.InternalServerException;
		//public bool IsInternalServerError => ResultStatus == ResultStatus.InternalServerException;


		public bool IsNotFoundError => ResultStatus == ResultStatus.NotFoundException;
		public bool IsInternalServerError => ResultStatus == ResultStatus.InternalServerException;

		//ForbiddenException, // 403
		//NotFoundException, // 404
		//ConflictException, // 409
		//UnprocessableEntityException, // 422
		//TooManyRequestsException, // 429
		//InternalServerException, // 500
		//BadGatewayException, // 502
		//ServiceUnavailableException, // 503
		//GatewayTimeoutException, // 504

		#endregion
	}
}
