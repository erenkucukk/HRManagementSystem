using HRManagementSystem.Application.Leave.DTOs;
using HRManagementSystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveController : ControllerBase
    {
        private readonly LeaveService _leaveService;

        public LeaveController(LeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        [HttpGet]
        public async Task<ActionResult<List<LeaveDto>>> GetAll()
        {
            var leaves = await _leaveService.GetAllLeavesAsync();
            return Ok(leaves);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveDto>> GetById(int id)
        {
            var leave = await _leaveService.GetLeaveByIdAsync(id);
            if (leave == null) return NotFound();
            return Ok(leave);
        }

        [HttpPost]
        public async Task<ActionResult<LeaveDto>> Create([FromBody] CreateLeaveDto dto)
        {
            var leave = await _leaveService.CreateLeaveAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = leave.Id }, leave);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLeaveDto dto)
        {
            var result = await _leaveService.UpdateLeaveAsync(id, dto);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _leaveService.DeleteLeaveAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}