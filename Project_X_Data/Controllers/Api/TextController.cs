using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Project_X_Data.Controllers.Api
{
    [Route("api/text")]
    [ApiController]
    public class TextController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = new { isOK = true },
                data = new { message = "Бек працює!" }
            });
        }
    }
}
