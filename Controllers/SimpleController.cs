using Microsoft.AspNetCore.Mvc;

namespace SimpleApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SimpleController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Message = "Hola, este es un API simple!" });
        }
    }
}
