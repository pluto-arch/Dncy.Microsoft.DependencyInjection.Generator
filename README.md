# 依赖注入代码自动生成器

1. [原生注入代码生成](https://github.com/pluto-arch/Dncy.Microsoft.DependencyInjection.Generator/blob/0f772d41226c6872a1e7aa7bc8c33f183545b713/DependencyInjection.Generator/AutoInject_README.md)

### 注入使用示例：
测试项目 ConsoleTest
```
 [Injectable(InjectLifeTime.Scoped)]
public class Demo
{
   public void Cw()
   {
      Console.WriteLine("Demo");
   }
}

 [Injectable(InjectLifeTime.Scoped, typeof(IServiceA))]
    public class ServiceA:IServiceA
    {
        public void Cw()
        {
            Console.WriteLine("IServiceA");
        }
    }
// 以上两个都会有被注入

var services = new ServiceCollection();
services.AutoInjectConsoleTest(); // 这里扩展名称是：AutoInject+程序集名称方式
```


2. [原生注入解析代码生成](https://github.com/pluto-arch/Dncy.Microsoft.DependencyInjection.Generator/blob/0f772d41226c6872a1e7aa7bc8c33f183545b713/DependencyInjection.Generator/ConstructorResolve_README.md)

### 构造函数解析使用示例：
```
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
```

## 打包
对应项目的Csproj中需要添加一下节点配置
```
<ItemGroup>
		<!-- Package the generator in the analyzer directory of the nuget package -->
		<None Include="$(OutputPath)\$(AssemblyName).dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
	</ItemGroup>
```



