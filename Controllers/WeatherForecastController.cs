using DotNet8Starter.Controllers.BaseController;
using Microsoft.AspNetCore.Mvc;

namespace DotNet8Starter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController() : BaseResultController
    {
        //private readonly IOrderService _nodejsService = nodejsService;


		private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };


        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

		//[HttpGet("GetTemperatureInKelvin/{temperatureInC}")]
		//public async Task<WeatherForecast> GetTemperatureInKelvin([FromRoute] int temperatureInC)
		//{
		//	var tempInK = await _nodejsService.GetKelvinTemperature(temperatureInC);

  //          return new WeatherForecast
  //          {
  //              Summary = "Random wather",
  //              TemperatureC = temperatureInC,
  //              TemperatureK = tempInK
  //          };
		//}
	}
}
