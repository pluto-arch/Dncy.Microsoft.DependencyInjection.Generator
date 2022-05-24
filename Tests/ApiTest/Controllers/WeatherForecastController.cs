using System.Dynamic;
using System.Runtime.InteropServices;
using ApiTest.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApiTest.Controllers
{
    [ApiController]
    [AutoResolveDependency]
    [Route("[controller]")]
    public partial class WeatherForecastController : ControllerBase
    {
        [AutoInject]
        private readonly ILogger<WeatherForecastController> _logger;

        [AutoInject]
        private readonly IBaseRepository<WeatherForecast> _baseRepository;

        [AutoInject]
        private readonly ICustomeRepository _customeRepository;


        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<string> Get()
        {
            return _baseRepository.GetList();
        }


        [HttpGet("list")]
        public IEnumerable<string> GetList()
        {
            return _customeRepository.GetList();
        }
    }

    [ApiController]
    [AutoResolveDependency]
    [Route("[controller]")]
    public partial class  DemoController
    {
        [AutoInject]
        private readonly ILogger<DemoController> _logger;

        [HttpGet]
        public IEnumerable<string> Get()
        {
            _logger.LogInformation("12323112");
            return new List<string>(){"123"};
        }

    }



    [AutoResolveDependency]
    public partial class  BaseController
    {
        [AutoInject]
        protected readonly ILogger<BaseController> _logger;
    }


    [ApiController]
    [AutoResolveDependency]
    [Route("[controller]")]
    public partial class  ValueController:ControllerBase
    {
        [AutoInject]
        private readonly IBaseRepository<WeatherForecast> _baseRepository;

        [AutoInject]
        private readonly ICustomeRepository _customeRepository;


        [HttpGet]
        public IEnumerable<string> Get()
        {
            var weatherForecasts = _baseRepository.GetList();
            return new List<string>() {"123"};
        }
    }
    
}