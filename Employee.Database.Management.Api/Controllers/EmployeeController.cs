
using EmailValidation;
using Employee.Database.Management.Database;
using Microsoft.AspNetCore.Mvc;

namespace Employee.Database.Management.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController:ControllerBase
    {
        private readonly ILogger<EmployeeController> _logger;
        private readonly IEmployeeDatabase _database;

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeDatabase database)
        {
            _logger = logger;
            _database = database;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            return Ok(await _database.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(Guid id)
        {
            try
            {
                var employee = await _database.GetEmployee(id);
                return Ok(employee);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Employee not found.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee([FromBody] Employee employee)
        {
            try
            {
                //validate email
                if (!EmailValidator.Validate(employee.Email))
                {
                    return BadRequest("Employee email is not valid.");
                }

                var res = await _database.AddEmployee(employee);
                return CreatedAtAction(nameof(GetEmployeeById), new { id = res.EmployeeId }, res);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] Employee updatedEmployee)
        {
            if (id != updatedEmployee.EmployeeId)
            {
                return BadRequest("Employee ID mismatch.");
            }

            //validate email
            if (!EmailValidator.Validate(updatedEmployee.Email))
            {
                return BadRequest("Employee email is not valid.");
            }

            try
            {
                await _database.UpdateEmployee(updatedEmployee);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Employee not found.");
            }
        }

    }
}
