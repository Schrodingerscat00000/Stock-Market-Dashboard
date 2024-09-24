using Microsoft.AspNetCore.Mvc;

namespace HelloWorldAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelloWorldController : ControllerBase
    {
        // GET api/helloworld
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello World");
        }
    }
}
