namespace DotNet8Starter.DL.Models
{
	public class ErrorResponse
	{
		public short Status { get; set; }
		public List<string> Errors { get; set; }
	}
}
