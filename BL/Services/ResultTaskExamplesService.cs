using DotNet8Starter.Exceptions.Validations;

namespace DotNet8Starter.BL.Services
{
	public class ResultTaskExamplesService
	{

		public async Task Starter()
		{
			var b = await DoWorkAsync();
		}

		public async Task<Result<string>> DoWorkAsync()
		{
			await Task.Delay(1000);
			return Result<string>.Success("Done after delay");
		}

		public ResultTask<string> DoWorkLater()
		{
			// Returns immediately with a ResultTask that wraps the asynchronous work
			return Task.Run(async () =>
			{
				await Task.Delay(1000);
				return Result<string>.Success("Done after delay");
			});
		}

	}
}
