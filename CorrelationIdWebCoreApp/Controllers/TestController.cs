using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CorrelationIdWebCoreApp.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private static readonly HttpClient HttpClient = new();

    [HttpGet]
    public async Task<string> Get()
    {
        await HttpClient.GetStringAsync("http://google.com");

        return "Test";
    }
}
