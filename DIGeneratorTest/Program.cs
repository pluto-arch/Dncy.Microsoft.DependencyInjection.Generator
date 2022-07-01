
using DIGeneratorTest;
using SA = DIGeneratorTest.ServiceA.DemoService;
using SB = DIGeneratorTest.ServiceB.DemoService;
using SC = DIGeneratorTest.ServiceC;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");




var services = new ServiceCollection();

services.AutoInjectDIGeneratorTest();
var provider = services.BuildServiceProvider();


var serA = provider.GetRequiredService<SA>();
serA.P();

var serb = provider.GetRequiredService<SB>();
serb.P();


var serc = provider.GetRequiredService<SC>();
serc.P();


Console.ReadKey();





