using HRManagementSystem.Application.Employees.DTOs;
using HRManagementSystem.Application.Leave.DTOs;
using HRManagementSystem.Domain.Entities;
using HRManagementSystem.Domain.Entities;
using HRManagementSystem.Domain.Enums;
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

        // Personelin sadece kendi izinlerini görebilmesi için
        public async Task<List<LeaveDto>> GetLeavesByEmployeeIdAsync(int employeeId)
        {
            var leaves = await _context.Leaves
                .Include(l => l.Employee)
                .Where(l => l.EmployeeId == employeeId)
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
                Status = l.Status.ToString(),
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            }).ToList();
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
                Status = LeaveStatus.Beklemede, // Enum kullanımı
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
                Status = leave.Status.ToString(),
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
                Status = l.Status.ToString(),
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            }).ToList();
        }

        public async Task<bool> UpdateLeaveAsync(int id, UpdateLeaveDto dto)
        {
            var leave = await _context.Leaves
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leave == null) return false;

            // 1. Eski izin gün sayısını hesapla ve personelden çıkar
            int oldLeaveDays = (int)(leave.EndDate.Date - leave.StartDate.Date).TotalDays + 1;
            if (oldLeaveDays < 0) oldLeaveDays = 0;

            // 2. Yeni izin gün sayısını hesapla
            int newLeaveDays = (int)(dto.EndDate.Date - dto.StartDate.Date).TotalDays + 1;
            if (newLeaveDays < 0) newLeaveDays = 0;

            // 3. UsedLeave güncellemesi
            if (leave.Employee != null)
            {
                leave.Employee.UsedLeave = leave.Employee.UsedLeave - oldLeaveDays + newLeaveDays;
                if (leave.Employee.UsedLeave < 0)
                    leave.Employee.UsedLeave = 0;
            }

            // 4. Leave güncelle
            leave.LeaveType = dto.LeaveType;
            leave.StartDate = dto.StartDate;
            leave.EndDate = dto.EndDate;
            leave.Reason = dto.Reason;
            leave.Status = Enum.TryParse<LeaveStatus>(dto.Status, out var statusEnum) ? statusEnum : leave.Status;
            leave.UpdatedAt = DateTime.UtcNow;

            _context.Update(leave);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteLeaveAsync(int id)
        {
            var leave = await _context.Leaves
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leave == null) return false;

            var employee = leave.Employee;

            // 1. UsedLeave azalt
            if (employee != null)
            {
                int leaveDays = (int)(leave.EndDate.Date - leave.StartDate.Date).TotalDays + 1;
                if (leaveDays < 0) leaveDays = 0;

                employee.UsedLeave -= leaveDays;
                if (employee.UsedLeave < 0)
                    employee.UsedLeave = 0; // Negatif olmasın

                // 2. Personelin başka aktif izni var mı kontrol et
                var now = DateTime.Now.Date;
                bool hasOtherActiveLeave = await _context.Leaves
                    .AnyAsync(l => l.EmployeeId == employee.Id && l.Id != leave.Id && l.EndDate.Date >= now);

                if (!hasOtherActiveLeave)
                {
                    employee.WorkingStatus = "Çalışıyor";
                    _context.Employees.Update(employee);
                }
            }

            _context.Leaves.Remove(leave);
            await _context.SaveChangesAsync();
            return true;
        }

        // HRManager izin onaylar
        public async Task<bool> ApproveLeaveAsync(int leaveId)
        {
            var leave = await _context.Leaves
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.Id == leaveId);

            if (leave == null || leave.Status == LeaveStatus.Izinli) return false;

            leave.Status = LeaveStatus.Izinli;
            leave.UpdatedAt = DateTime.UtcNow;

            // Kullanılan izin güncellemesi
            var employee = leave.Employee;
            if (employee != null)
            {
                int leaveDays = (int)(leave.EndDate.Date - leave.StartDate.Date).TotalDays + 1;
                if (leaveDays < 0) leaveDays = 0;
                employee.UsedLeave += leaveDays;
                employee.WorkingStatus = "İzinli";
                _context.Employees.Update(employee);
            }

            _context.Leaves.Update(leave);
            await _context.SaveChangesAsync();
            return true;
        }

        // HRManager izin reddeder
        public async Task<bool> RejectLeaveAsync(int leaveId)
        {
            var leave = await _context.Leaves
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.Id == leaveId);

            if (leave == null || leave.Status == LeaveStatus.Reddedildi) return false;

            leave.Status = LeaveStatus.Reddedildi;
            leave.UpdatedAt = DateTime.UtcNow;

            _context.Leaves.Update(leave);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task AutoUpdateEmployeeStatusAsync()
        {
            var today = DateTime.UtcNow.Date;

            // Bugün veya daha önce biten ve hâlâ "İzinli" olan izinleri bul
            var expiredLeaves = await _context.Leaves
                .Where(l => l.Status == LeaveStatus.Izinli && l.EndDate < today)
                .ToListAsync();

            foreach (var leave in expiredLeaves)
            {
                // İlgili employee'yi bul
                var employee = await _context.Employees.FindAsync(leave.EmployeeId);
                if (employee != null && employee.WorkingStatus != "Çalışıyor")
                {
                    employee.WorkingStatus = "Çalışıyor";
                }

                // Leave kaydının statüsü de istersek güncellenebilir
                leave.Status = LeaveStatus.Tamamlandi;
            }

            await _context.SaveChangesAsync();
        }
    }
}