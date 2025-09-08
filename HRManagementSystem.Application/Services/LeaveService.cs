using HRManagementSystem.Application.Employees.DTOs;
using HRManagementSystem.Application.Leave.DTOs;
using HRManagementSystem.Domain.Entities;
using HRManagementSystem.Domain.Entities;
using HRManagementSystem.Infrastructure.Persistence;
using HRManagementSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // 1. Employee kontrolü
            var employee = await _context.Employees.FindAsync(dto.EmployeeId);
            if (employee == null)
                throw new ArgumentException($"Employee with Id {dto.EmployeeId} does not exist.");

            // 2. Leave nesnesini oluştur
            var leave = new Domain.Entities.Leave
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

            // 3. Veritabanına ekle
            _context.Leaves.Add(leave);
            await _context.SaveChangesAsync();

            // 4. DTO olarak geri döndür
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

        public async Task<bool> UpdateLeaveAsync(int id, UpdateLeaveDto dto)
        {
            var leave = await _context.Leaves.FindAsync(id);
            if (leave == null) return false;

            leave.LeaveType = dto.LeaveType;
            leave.StartDate = dto.StartDate;
            leave.EndDate = dto.EndDate;
            leave.Reason = dto.Reason;
            leave.Status = dto.Status;
            leave.UpdatedAt = DateTime.UtcNow;

            _context.Update(leave);
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