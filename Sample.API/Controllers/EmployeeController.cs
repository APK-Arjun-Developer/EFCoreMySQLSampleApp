using Microsoft.AspNetCore.Mvc;
using Sample.Context;
using Sample.Model;

namespace Sample.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _employeeService.GetEmployees();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var employee = await _employeeService.GetEmployee(id);
            if (employee == null) return NotFound($"Employee with ID {id} not found.");
            return Ok(employee);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] Employee employeeModel)
        {
            if (employeeModel == null) return BadRequest("Employee data is required.");

            var employee = ExpressMapper.Mapper.Map<Employee, EmployeeEntity>(employeeModel);
            var success = await _employeeService.CreateEmployee(employee);
            if (!success) return StatusCode(500, "Error creating employee.");

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.EmployeeId }, employee);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] Employee employeeModel)
        {
            if (id <= 0) return BadRequest("Invalid employee ID.");

            if (employeeModel == null) return BadRequest("Invalid employee data.");

            var employee = ExpressMapper.Mapper.Map<Employee, EmployeeEntity>(employeeModel);
            employee.EmployeeId = id;
            var success = await _employeeService.UpdateEmployee(employee);
            if (!success) return NotFound($"Employee with ID {id} not found.");

            return Ok(employee);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var success = await _employeeService.DeleteEmployee(id);
            if (!success) return NotFound($"Employee with ID {id} not found.");

            return NoContent();
        }
    }
}
