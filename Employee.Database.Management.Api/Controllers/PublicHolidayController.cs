using Employee.Database.Management.Service;
using Microsoft.AspNetCore.Mvc;

namespace Employee.Database.Management.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PublicHolidayController:ControllerBase
    {
        private readonly ILogger<PublicHolidayController> _logger;
        private readonly IPublicHolidayService _publicHolidayService;
        public PublicHolidayController(ILogger<PublicHolidayController> logger, IPublicHolidayService publicHolidayService)
        {
            _logger = logger;
            _publicHolidayService = publicHolidayService;
        }

        [HttpGet("country/{countryCode}")]
        public async Task<IActionResult> GetPublicHolidayByCountryCode(string countryCode)
        {
            var publicHoliday = await _publicHolidayService.GetPublicHolidayByCountryCodeAndYear(countryCode, DateTime.Now.Year);

            return Ok(new { publicHoliday });
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetPublicHolidayForCurrent7DaysByEmployee(Guid employeeId)
        {
            var publicHoliday = await _publicHolidayService.GetPublicHolidayForCurrent7DaysByEmployee(employeeId);

            return Ok(new { publicHoliday });
        }
    }

    
}