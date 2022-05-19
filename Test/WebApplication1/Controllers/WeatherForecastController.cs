using AppServices;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IBlogAppService _blogAppService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IBlogAppService blogAppService)
        {
            _logger = logger;
            _blogAppService = blogAppService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<string> Get()
        {
            return _blogAppService.GetList();
        }
    }
}