namespace Samples.Web;

public class ExampleService
{
    private readonly HttpClient _httpClient;
    public ExampleService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    // registration of the http client in this way uses the Typed client
}