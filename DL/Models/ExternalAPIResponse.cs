using System.Net;

namespace DotNet8Starter.DL.Models
{
	public class ExternalAPIResponse<T> where T : class
	{
		public bool IsSuccessful { get; set; }
		public bool IsEligibleForRetry { get; set; }
		public string? ErrorMessage { get; set; }
		public short HTTP_Status {  get; set; }
		public T? ResponseBody { get; set; }

		public static ExternalAPIResponse<T> ToSuccessfulResponse(T response, HttpStatusCode httpStatus = HttpStatusCode.OK)
		{
			return new ExternalAPIResponse<T>
			{
				IsSuccessful = true,
				IsEligibleForRetry = false,
				HTTP_Status = (short)httpStatus,
				ResponseBody = response
			};
		}

		public static ExternalAPIResponse<T> ToFailedResponse(bool isEligibleForRetry, string errorMessage, HttpStatusCode httpStatus)
		{
			return new ExternalAPIResponse<T>
			{
				IsSuccessful = false,
				IsEligibleForRetry = isEligibleForRetry,
				HTTP_Status = (short)httpStatus,
				ErrorMessage = errorMessage
			};
		}
	}
}
