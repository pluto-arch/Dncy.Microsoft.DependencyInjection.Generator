
using System.Diagnostics;
using DIGeneratorTest;
using SA = DIGeneratorTest.ServiceA.DemoService;
using SB = DIGeneratorTest.ServiceB.DemoService;
using SC = DIGeneratorTest.ServiceC;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


Console.WriteLine("Hello, World!");


var services = new ServiceCollection();
services.AddScoped<ILogProxyFactory, LogProxyFactory>();
services.AddScoped<ProxyContext>();
services.AddScoped<SerAProxy>();
services.AddScoped<SerA>();
services.AddScoped<D>();
services.AddSingleton<IDiagnosticListener, HttpClientDiagnosticListener>();
services.AddSingleton<IObserver<DiagnosticListener>, Lis>();
//services.AutoInjectDIGeneratorTest();
var provider = services.BuildServiceProvider();

var sdsd = provider.GetRequiredService<D>();
sdsd.Do();

//var serA = provider.GetRequiredService<SA>();
//serA.P();

//var serb = provider.GetRequiredService<SB>();
//serb.P();


//var serc = provider.GetRequiredService<SC>();
//serc.P();


DiagnosticListener.AllListeners.Subscribe(provider.GetRequiredService<IObserver<DiagnosticListener>>());
using var client=new HttpClient(); 
var response =await client.GetAsync("http://www.baidu.com");
var ts =  await  response.Content.ReadAsStringAsync();

Console.ReadKey();

public class Lis : IObserver<DiagnosticListener>
{

    private IEnumerable<IDiagnosticListener> _listeners;

    public Lis(IEnumerable<IDiagnosticListener> listeners)
    {
        _listeners = listeners;
    }

    /// <inheritdoc />
    public void OnCompleted()
    {
    }

    /// <inheritdoc />
    public void OnError(Exception error)
    {
    }

    /// <inheritdoc />
    public void OnNext(DiagnosticListener value)
    {
        IDiagnosticListener diagnosticListener = _listeners.Where(x => x.ListenerName == value.Name).FirstOrDefault();
        if (diagnosticListener != null)
        {
            value.Subscribe(diagnosticListener);  
        }
    }
}


public class D
{
    private readonly SerA _s;
    private readonly ILogProxyFactory _factory;

    public D(SerA s,ILogProxyFactory factory)
    {
        _s = s;
        _factory = factory;
    }

    public void Do()
    {
        Console.Write(_s.WithLogging(_factory).Get());
    }
    

}


public interface ILogProxyFactory
{
    T Create<T>() where T : notnull;
}


public class LogProxyFactory:ILogProxyFactory
{
    private readonly IServiceProvider _service;

    public LogProxyFactory(IServiceProvider service)
    {
        _service = service;
    }

    public T Create<T>() where T : notnull
    {
        return _service.GetRequiredService<T>();
    }
}



public class SerA
{
    public string Get()
    {
        return "12312";
    }
}

public static class SerAEx
{
    public static SerAProxy WithLogging(this SerA _,ILogProxyFactory factory)
    {
        return factory.Create<SerAProxy>();
    }
}


public class SerAProxy:SerA
{
    private readonly ProxyContext _ctx;

    public SerAProxy(ProxyContext ctx)
    {
        _ctx = ctx;
    }

    public new string Get()
    {
        _ctx.Enter(nameof(Get));
        try
        {
            return base.Get();
        }
        catch
        {
            throw;
        }
        finally
        {
            _ctx.Leave(nameof(Get));
        }
    }
}




public class ProxyContext
{
    private readonly AsyncLocal<TraceInfo> _list;

    public ProxyContext()
    {
        _list=new AsyncLocal<TraceInfo>(ValueChangedHandler);
    }

    private void ValueChangedHandler(AsyncLocalValueChangedArgs<TraceInfo> obj)
    {
        try
        {
            if (obj.ThreadContextChanged)
            {
            }
            Console.WriteLine($@"this old value:{obj.PreviousValue?.MethodName}. the current value: {obj.CurrentValue?.MethodName}");
        }
        catch(Exception e)
        {
            Console.WriteLine("APM han an error :{msg}",e.Message);
        }
    }


    public class TraceInfo
    {
        public TraceInfo()
        {
            MethodName = string.Empty;
        }

        public TraceInfo(string name)
        {
            MethodName = name;
        }

        public string MethodName { get; set; }
    }

    public void Enter(string methodName)
    {
        _list.Value = new TraceInfo(methodName);
    }

    public void Leave(string methodName)
    {
        _list.Value = new TraceInfo(methodName);
    }
}