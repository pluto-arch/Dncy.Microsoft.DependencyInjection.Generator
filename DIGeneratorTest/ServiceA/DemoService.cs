using Microsoft.Extensions.DependencyInjection;

namespace DIGeneratorTest.ServiceA;

[Injectable(InjectLifeTime.Singleton)]
[AutoResolveDependency]
public partial class DemoService
{
    [AutoInject]
    private readonly Demo _demo;

    public void P()
    {
        this._demo.Print("DIGeneratorTest.ServiceA.DemoService");
    }
}