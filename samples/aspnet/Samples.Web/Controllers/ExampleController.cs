using Microsoft.AspNetCore.Mvc;
using Telepresence.NET.RestfulApi;

namespace Samples.Web.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class ExampleController(ITelepresenceApiService apiService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Test()
    {
        var healthyTask = apiService.Healthz();
        var consumeHereTask = apiService.ConsumeHere();
        var interceptInfoTask = apiService.InterceptInfo();

        var response = new
        {
            healthy = await healthyTask,
            consumeHere = await consumeHereTask,
            interceptInfo = await interceptInfoTask
        };

        return Ok(response);
    }
}
