using Microsoft.Extensions.DependencyInjection;

namespace DIGeneratorTest.ServiceB;

[Injectable(InjectLifeTime.Singleton)]
[AutoResolveDependency]
public partial class DemoService
{
    [AutoInject]
    private readonly Demo _demo;

    public void P()
    {
        this._demo.Print("DIGeneratorTest.ServiceB.DemoService");
    }
}