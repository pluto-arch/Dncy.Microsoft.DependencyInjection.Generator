using Microsoft.Extensions.DependencyInjection;

namespace DIGeneratorTest;


[Injectable(InjectLifeTime.Singleton)]
[AutoResolveDependency]
public partial class ServiceC:ServiceA.DemoService
{
    [AutoInject]
    private readonly DemoA _demoA;

}