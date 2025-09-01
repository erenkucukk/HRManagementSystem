using HRManagementSystem.Application.Common;
using HRManagementSystem.Application.DTOs;
using HRManagementSystem.Application.Employees;
using HRManagementSystem.Application.Employees.DTOs;
using HRManagementSystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Tüm endpointler için yetkilendirme gerekli

    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeService _employeeService;

        public EmployeeController(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<ActionResult<List<EmployeeDto>>> GetAll()
        {
            var employees = await _employeeService.GetAllAsync();
            return Ok(employees);
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<EmployeeDto>>> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? departmentId = null,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = "HireDate",
            [FromQuery] string? sortDir = "desc")
        {
            var result = await _employeeService.GetPagedAsync(new EmployeeQuery
            {
                Page = page,
                PageSize = pageSize,
                DepartmentId = departmentId,
                Search = search,
                SortBy = sortBy,
                SortDir = sortDir
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> GetById(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null)
                return NotFound();
            return Ok(employee);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateEmployeeDto dto)
        {
            var id = await _employeeService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
        {
            var result = await _employeeService.UpdateAsync(id, dto);
            if (!result)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _employeeService.DeleteAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
    }
}
