using HRManagementSystem.Application.Leave.DTOs;
using HRManagementSystem.Application.Services;
using HRManagementSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/personnel/leave")]
    [Authorize(Roles = "Personnel")]
    public class PersonnelLeaveController : ControllerBase
    {
        private readonly LeaveService _leaveService;
        private readonly UserService _userService;

        public PersonnelLeaveController(LeaveService leaveService, UserService userService)
        {
            _leaveService = leaveService;
            _userService = userService;
        }

        [HttpGet("my-leaves")]
        public async Task<ActionResult<List<LeaveDto>>> GetMyLeaves()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var employeeId = await _userService.GetEmployeeIdByUserIdAsync(userId);
            if (employeeId == null) return NotFound("Employee not found for this user.");

            var leaves = await _leaveService.GetLeavesByEmployeeIdAsync(employeeId.Value);
            return Ok(leaves);
        }

        [HttpPost]
        public async Task<ActionResult<LeaveDto>> Create([FromBody] CreateLeaveDto dto)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var employeeId = await _userService.GetEmployeeIdByUserIdAsync(userId);
            if (employeeId == null) return NotFound("Employee not found for this user.");

            dto.EmployeeId = employeeId.Value;
            var leave = await _leaveService.CreateLeaveAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = leave.Id }, leave);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LeaveDto>> GetById(int id)
        {
            var leave = await _leaveService.GetLeaveByIdAsync(id);
            if (leave == null) return NotFound();

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var employeeId = await _userService.GetEmployeeIdByUserIdAsync(userId);
            if (leave.EmployeeId != employeeId) return Forbid();

            return Ok(leave);
        }

        // (Gelişmiş: sadece kendi beklemedeki izinleri için güncelle/sil ekleyebilirsin)
    }
}