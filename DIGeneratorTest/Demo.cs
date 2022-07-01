using Microsoft.Extensions.DependencyInjection;

namespace DIGeneratorTest;


[Injectable(InjectLifeTime.Singleton)]
public class Demo
{
    public void Print(string msg)
    {
        Console.WriteLine(msg);
    }
}