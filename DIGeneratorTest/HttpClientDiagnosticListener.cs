using System.Net.Http.Json;
using System.Text.Json;

namespace DIGeneratorTest;

public class HttpClientDiagnosticListener:IDiagnosticListener
{
    /// <inheritdoc />
    public void OnCompleted()
    {
    }

    /// <inheritdoc />
    public void OnError(Exception error)
    {
    }

    /// <inheritdoc />
    public void OnNext(KeyValuePair<string, object> value)
    {
        if (value.Key == "System.Net.Http.HttpRequestOut.Start")
        {
            HandleHttpRequest(value.Value);
        }  
    }

    private void HandleHttpRequest(object value)
    {
        var request = value.GetType().GetProperty("Request")?.GetValue(value) as System.Net.Http.HttpRequestMessage;
        Console.WriteLine(JsonSerializer.Serialize(request.Headers));
    }

    /// <inheritdoc />
    public string ListenerName => "HttpHandlerDiagnosticListener";
}



public interface IDiagnosticListener : IObserver<KeyValuePair<string, object>>
{
    string ListenerName { get; }  

}