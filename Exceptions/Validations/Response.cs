using System.Net;

namespace DotNet8Starter.Exceptions.Validations
{

	// TODO Unit tests
	// TODO StyleCop.Analyzer
	// TODO desktop notepad Result updates.txt

	public record Response
	{
		public bool IsSuccessful { get; }
		public bool IsError => !IsSuccessful;
		public HttpStatusCode Status { get; }
		public RError RError { get; }

		/// <summary>
		/// Create a Response for successful cases
		/// </summary>
		/// <param name="status">HTTP Status Code</param>
		protected Response(HttpStatusCode status)
		{
			IsSuccessful = true;
			Status = status;
			RError = RError.NoError();
		}

		/// <summary>
		/// Create a Response for exception cases
		/// </summary>
		/// <param name="error">RError instance</param>
		protected Response(RError error)
		{
			IsSuccessful = false;
			RError = error;
			Status = error.StatusCode;
		}

		/// <summary>
		/// Explicit way to pass a successful outcome
		/// </summary>
		/// <returns></returns>
		public static Response Success() => new(HttpStatusCode.OK);

		/// <summary>
		/// Successful outcome with status 201 created
		/// </summary>
		/// <returns></returns>
		public static Response Created() => new(HttpStatusCode.Created);

		/// <summary>
		/// Successful outcome with status 204 no content
		/// </summary>
		/// <returns></returns>
		public static Response Empty() => new(HttpStatusCode.NoContent);


		// Errors on the Response (because why not)

		/// <summary>
		/// Create new generic error instance
		/// </summary>
		/// <returns></returns>
		public static Response Error() => new(RError.NewRError());

		/// <summary>
		/// For try catch
		/// </summary>
		/// <param name="ex"></param>
		/// <returns></returns>
		public static Response Error(Exception ex) => new(RError.NewRError(ex.Message));

		/// <summary>
		/// Create a new generic error instance with custom message
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static Response Error(string message) => new(RError.NewRError(message));

		/// <summary>
		/// HTTP Based Errors
		/// </summary>
		/// <param name="status"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public static Response Error (int status, string message) => new (RError.HttpError(status, message));

		public static Response Error(HttpStatusCode status, string message) => new(RError.HttpError(status, message));


		// Transfer error state between types

		/// <summary>
		/// Transfer error from one generic type to another
		/// </summary>
		/// <param name="response">Response with error.</param>
		/// <returns>New Response while preserving the existing Error.</returns>
		/// <exception cref="ArgumentException">If the response does not contain error.</exception>
		public static Response TransferError(Response response)
		{
			if (!response.IsError || response.RError is null)
				throw new ArgumentException("TransferError requires a Response containing an error.");

			return new Response(response.RError);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="response"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static Response<T> TransferError<T>(Response response)
		{
			if (!response.IsError || response.RError is null)
				throw new ArgumentException("TransferError requires a Response containing an error.");

			return new Response<T>(response.RError);
		}

		/// ...
		/// 

		public Response<T> To<T>() =>
			IsError ? new Response<T>(RError!) : throw new InvalidOperationException();

		// lets you do this:
		//return respErr.To<string>();


		#region implicit operators

		public static implicit operator Response(RError error) =>
			new(error);

		public static implicit operator Response(Exception ex) =>
			new(new RError(HttpStatusCode.InternalServerError, ex.Message));

		public static implicit operator Response(ApplicationException ex) =>
			new(new RError(HttpStatusCode.BadRequest, ex.Message));

		#endregion
	}

	public record Response<T> : Response
	{
		public T? Value { get; }

		private Response(T value, HttpStatusCode status = HttpStatusCode.OK)
			: base(status) => Value = value;

		internal Response(RError error) : base(error) { }

		public static implicit operator Response<T>(T value) =>
			new(value);

		public static implicit operator Response<T>(RError error) =>
			new(error);

		public static Response<T> Success(T value) =>
			new(value, HttpStatusCode.OK);

		public static Response<T> Created(T value) =>
			new(value, HttpStatusCode.Created);

		// because I want to use it with Response<T>
		public static new Response<T> Empty() => (Response<T>)Response.Empty();



		// Jolda's idea => Error Payload
		//public static Response Error(T payload) => new(RError.NewRError());

		// Error(ex) => should ex be payload?

		// or simply .Problem()
		// ErrorAsProblemDetails

		// match
		public TResult Match<TResult>(
			Func<T, TResult> onSuccess,
			Func<RError, TResult> onError)
		{
			return IsError ? onError(RError!) : onSuccess(Value!);
		}

		// MAP
		public Response<TResult> Map<TResult>(Func<T, TResult> mapper)
		{
			return IsError
				? new Response<TResult>(RError!)
				: new Response<TResult>(mapper(Value!));
		}

		//Response<User> resp = GetUser(id);

		//Response<UserDto> dtoResp = resp.Map(user => new UserDto(user));

		// switch
		public void Switch(
			Action<T> onSuccess,
			Action<RError> onError)
		{
			if (IsError)
				onError(RError!);
			else
				onSuccess(Value!);
		}

		//resp.Switch(
		//	success => Log.Info("OK"),
		//	error   => Log.Warn(error.Message)
		//);

		// BIND

		public Response<TResult> Bind<TResult>(Func<T, Response<TResult>> binder)
		{
			return IsError
				? new Response<TResult>(RError!)
				: binder(Value!);
		}

		//public Response<WeatherForecast> GetWeatherBySummary(string summary)
		//{
		//	return CheckSummary(summary)
		//		.Bind(valid => LoadWeather(valid))
		//		.Map(weather => Transform(weather))
		//		.Match(
		//			success => Response.Success(success),
		//			error => Response.TransferError<WeatherForecast>(error)
		//		);
		//}


		/// Errors february
		/// 

		// Errors on the Response (because why not)

		/// new is here not to confuse Response.Error() with Response<T>.Error
		public static new Response<T> Error() => new(RError.NewRError());

		public static new Response<T> Error(string message) => new(RError.NewRError(message));

		public static new Response<T> Error(int status, string message) => new(RError.HttpError(status, message));

		public static new Response<T> Error(HttpStatusCode status, string message) => new(RError.HttpError(status, message));





	}



}

/*
 *
 *		private Response(T value, HttpStatusCode status) : base(null)
		{
			Value = value;
			Status = status;
		}

		public static Response Success(T value) => new Response<T> (value, HttpStatusCode.OK);
		public static Response Created(T value) => new Response<T> (value, HttpStatusCode.Created);
 **/