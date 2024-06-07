using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverterApp.Controllers
{
    [ApiController]
    [Route("/error")]
    public class ExceptionController: ControllerBase
    {
        [HttpGet]
        public IActionResult HandleError()
        {
            return Problem();
        }
    }
}
