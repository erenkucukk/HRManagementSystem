using HRManagementSystem.Application.Leave.DTOs;
using HRManagementSystem.Domain.Entities;
using HRManagementSystem.Infrastructure.Persistence;
using HRManagementSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRManagementSystem.Domain.Entities;

namespace HRManagementSystem.Application.Services
{
    public class LeaveService
    {
        private readonly HRDbContext _context;

        public LeaveService(HRDbContext context)
        {
            _context = context;
        }

        public async Task<LeaveDto> CreateLeaveAsync(CreateLeaveDto dto)
        {
            var leave = new Domain.Entities.Leave()
            {
                EmployeeId = dto.EmployeeId,
                LeaveType = dto.LeaveType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Reason = dto.Reason,
                Status = "Approved", // HR ekliyorsa direkt onaylı
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Leaves.Add(leave);
            await _context.SaveChangesAsync();

            return await GetLeaveByIdAsync(leave.Id);
        }

        public async Task<LeaveDto?> GetLeaveByIdAsync(int id)
        {
            var leave = await _context.Leaves
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leave == null) return null;

            return new LeaveDto
            {
                Id = leave.Id,
                EmployeeId = leave.EmployeeId,
                EmployeeName = leave.Employee != null
                    ? $"{leave.Employee.FirstName} {leave.Employee.LastName}"
                    : "",
                LeaveType = leave.LeaveType,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                Reason = leave.Reason,
                Status = leave.Status,
                CreatedAt = leave.CreatedAt,
                UpdatedAt = leave.UpdatedAt
            };
        }

        public async Task<List<LeaveDto>> GetAllLeavesAsync()
        {
            var leaves = await _context.Leaves
                .Include(l => l.Employee)
                .OrderByDescending(l => l.StartDate)
                .ToListAsync();

            return leaves.Select(l => new LeaveDto
            {
                Id = l.Id,
                EmployeeId = l.EmployeeId,
                EmployeeName = l.Employee != null
                    ? $"{l.Employee.FirstName} {l.Employee.LastName}"
                    : "",
                LeaveType = l.LeaveType,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Reason = l.Reason,
                Status = l.Status,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            }).ToList();
        }

        public async Task<bool> UpdateLeaveStatusAsync(int id, UpdateLeaveDto dto)
        {
            var leave = await _context.Leaves.FindAsync(id);
            if (leave == null) return false;

            leave.Status = dto.Status;
            leave.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteLeaveAsync(int id)
        {
            var leave = await _context.Leaves.FindAsync(id);
            if (leave == null) return false;

            _context.Leaves.Remove(leave);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}