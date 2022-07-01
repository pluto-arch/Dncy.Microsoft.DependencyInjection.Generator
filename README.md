# 依赖注入代码自动生成器

1. [原生注入代码生成](https://github.com/pluto-arch/Dncy.Microsoft.DependencyInjection.Generator/blob/main/Dncy.DependencyInjection.Generator/AutoInject_README.md)

### 安装包
```
PM> Install-Package Dncy.DependencyInjection.Generator -Version 1.0.0
```

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

2. [原生注入解析代码生成](https://github.com/pluto-arch/Dncy.Microsoft.DependencyInjection.Generator/blob/main/Dncy.DependencyInjection.Generator/ConstructorResolve_README.md)

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
> 支持类继承的解析

## 源码调试

在调试的时候，目标项目需要引入生成器先项目，然后再 csproj 中加入：

```
<!--OutputItemType 必须为Analyzer 分析器  打包以后就不需要了-->
<ItemGroup>
    <ProjectReference Include="..\..\DependencyInjection.Generator\Dncy.MicrosoftDependencyInjection.Generator.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
  </ItemGroup>
```

然后在生成器的`Initialize`方法中使用`Debugger`

```
if (!Debugger.IsAttached)
{
    Debugger.Launch();
}
```

> 调试的时候 可能更改完不会生成最新的目标代码，需要重启 vs，重新打开项目。
> 打包后引入就不会出现这种问题了。

## 打包

对应项目的 Csproj 中需要添加一下节点配置

```
<ItemGroup>
		<!-- Package the generator in the analyzer directory of the nuget package -->
		<None Include="$(OutputPath)\$(AssemblyName).dll" Pack="true" PackagePath="analyzers/dotnet/cs" Visible="false" />
	</ItemGroup>
```
