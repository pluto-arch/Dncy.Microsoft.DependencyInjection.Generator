using System.Dynamic;
using System.Runtime.InteropServices;
using ApiTest.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApiTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;

        private readonly IBaseRepository<WeatherForecast> _baseRepository;

        private readonly ICustomeRepository _customeRepository;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IBaseRepository<WeatherForecast> baseRepository, ICustomeRepository customeRepository)
        {
            _logger = logger;
            _baseRepository = baseRepository;
            _customeRepository = customeRepository;
        }

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
    [AutoController]
    [Route("[controller]")]
    public partial class  DemoController
    {
        private readonly ILogger<DemoController> _logger;

        [HttpGet]
        public IEnumerable<string> Get()
        {
            _logger.LogInformation("12323112");
            return new List<string>(){"123"};
        }

    }



   
    [AutoController]
    public partial class  BaseController
    {
        protected readonly ILogger<BaseController> _logger;
    }


    [ApiController]
    [AutoController]
    [Route("[controller]")]
    public  class  ValueController:BaseController
    {

        [HttpGet]
        public IEnumerable<string> Get()
        {
            _logger.LogInformation("12323112");
            return new List<string>(){"123"};
        }


        /// <inheritdoc />
        public ValueController(ILogger<BaseController> __logger) : base(__logger)
        {
        }
    }


}