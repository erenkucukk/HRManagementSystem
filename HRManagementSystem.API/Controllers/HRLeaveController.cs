using HRManagementSystem.Application.Leave.DTOs;
using HRManagementSystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/hr/leave")]
    [Authorize(Roles = "HRManager")]
    public class HRLeaveController : ControllerBase
    {
        private readonly LeaveService _leaveService;

        public HRLeaveController(LeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<LeaveDto>>> GetAll()
        {
            var leaves = await _leaveService.GetAllLeavesAsync();
            return Ok(leaves);
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _leaveService.ApproveLeaveAsync(id);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _leaveService.RejectLeaveAsync(id);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveDto>> GetById(int id)
        {
            var leave = await _leaveService.GetLeaveByIdAsync(id);
            if (leave == null) return NotFound();
            return Ok(leave);
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