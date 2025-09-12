using HRManagementSystem.Application.Common;
using HRManagementSystem.Application.DTOs;
using HRManagementSystem.Application.Employees.DTOs;
using HRManagementSystem.Application.Expense.DTOs;
using HRManagementSystem.Domain.Entities;
using HRManagementSystem.Infrastructure.Persistence;
using HRManagementSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HRManagementSystem.Application.Services
{
    public class EmployeeService
    {
        private readonly HRDbContext _context;
        private readonly IRepository<Employee> _employeeRepository;

        public EmployeeService(IRepository<Employee> employeeRepository, HRDbContext context)
        {
            _employeeRepository = employeeRepository;
            _context = context;
        }

        public async Task<List<EmployeeDto>> GetAllAsync()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.ExpenseHistories)
                    .ThenInclude(h => h.Receipts)
                .ToListAsync();

            return employees
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    TCKimlik = e.TCKimlik,
                    DogumTarihi = e.DogumTarihi,
                    TelNo = e.TelNo,
                    Email = e.Email,
                    Position = e.Position,
                    WorkingStatus = e.WorkingStatus,
                    PersonnelPhoto = e.PersonnelPhoto,
                    Adres = e.Adres,
                    StartDate = e.StartDate,
                    TotalLeave = e.TotalLeave,
                    UsedLeave = e.UsedLeave,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = e.Department?.Name ?? string.Empty,
                    Salary = e.Salary,
                    MealCost = e.MealCost,
                    TransportCost = e.TransportCost,
                    OtherCost = e.OtherCost,
                    ExpenseHistory = e.ExpenseHistories?
                        .Select(h => new ExpenseHistoryDto
                        {
                            Id = h.Id,
                            Amount = h.Amount,
                            Date = h.Date,
                            MealCost = h.MealCost,
                            TransportCost = h.TransportCost,
                            OtherCost = h.OtherCost,
                            ReceiptUrls = h.Receipts?.Select(r => r.FileUrl).ToList() ?? new List<string>()
                        }).ToList() ?? new List<ExpenseHistoryDto>()
                })
                .ToList();
        }

        public async Task<EmployeeDto?> GetByIdAsync(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return null;

            return new EmployeeDto
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                TCKimlik = employee.TCKimlik,
                DogumTarihi = employee.DogumTarihi,
                TelNo = employee.TelNo,
                Email = employee.Email,
                Position = employee.Position,
                WorkingStatus = employee.WorkingStatus,
                Adres = employee.Adres,
                PersonnelPhoto = employee.PersonnelPhoto,
                StartDate = employee.StartDate,
                TotalLeave = employee.TotalLeave,
                UsedLeave = employee.UsedLeave,
                DepartmentId = employee.DepartmentId,
                DepartmentName = employee.Department?.Name ?? string.Empty,
                Salary = employee.Salary,
                MealCost = employee.MealCost,
                TransportCost = employee.TransportCost,
                OtherCost = employee.OtherCost
            };
        }

        public async Task<int> CreateAsync(CreateEmployeeDto dto)
        {
            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                TCKimlik = dto.TCKimlik,
                DogumTarihi = dto.DogumTarihi,
                TelNo = dto.TelNo,
                Email = dto.Email,
                Adres = dto.Adres,
                Position = dto.Position,
                WorkingStatus = dto.WorkingStatus,
                PersonnelPhoto = dto.PersonnelPhoto,
                StartDate = dto.StartDate,
                TotalLeave = dto.TotalLeave,
                UsedLeave = dto.UsedLeave,
                DepartmentId = dto.DepartmentId,

            };

            await _employeeRepository.AddAsync(employee);
            await _employeeRepository.SaveChangesAsync();
            return employee.Id;
        }

        public async Task<bool> UpdateAsync(int id, UpdateEmployeeDto dto)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null) return false;

            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.TCKimlik = dto.TCKimlik;
            employee.DogumTarihi = dto.DogumTarihi;
            employee.TelNo = dto.TelNo;
            employee.Email = dto.Email;
            employee.Adres = dto.Adres;
            employee.Position = dto.Position;
            employee.WorkingStatus = dto.WorkingStatus;
            employee.PersonnelPhoto = dto.PersonnelPhoto;
            employee.StartDate = dto.StartDate;
            employee.TotalLeave = dto.TotalLeave;
            employee.UsedLeave = dto.UsedLeave;
            employee.DepartmentId = dto.DepartmentId;

            _employeeRepository.Update(employee);
            await _employeeRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null) return false;

            _employeeRepository.Remove(employee);
            await _employeeRepository.SaveChangesAsync();
            return true;
        }

        public async Task<ExpenseHistoryDto?> AddCostAsync(int employeeId, UpdateCostDto dto)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null) return null;

            employee.Salary += dto.Salary ?? 0;
            employee.MealCost += dto.MealCost ?? 0;
            employee.TransportCost += dto.TransportCost ?? 0;
            employee.OtherCost += dto.OtherCost ?? 0;

            var totalExpense = (dto.Salary ?? 0) + (dto.MealCost ?? 0) + (dto.TransportCost ?? 0) + (dto.OtherCost ?? 0);

            var expenseHistory = new ExpenseHistory
            {
                Amount = totalExpense,
                Date = dto.ExpenseDate,
                Receipts = new List<ExpenseReceipt>()
            };

            if (dto.Receipts != null && dto.Receipts.Any())
            {
                foreach (var file in dto.Receipts)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var uploads = Path.Combine("wwwroot", "uploads", fileName);
                    using (var fs = new FileStream(uploads, FileMode.Create))
                    {
                        await file.CopyToAsync(fs);
                    }
                    expenseHistory.Receipts.Add(new ExpenseReceipt { FileUrl = "/uploads/" + fileName });
                }
            }

            employee.ExpenseHistories.Add(expenseHistory);
            _employeeRepository.Update(employee);
            await _employeeRepository.SaveChangesAsync();

            // Dönüş için oluştur:
            return new ExpenseHistoryDto
            {
                Id = expenseHistory.Id,
                Amount = expenseHistory.Amount,
                Date = expenseHistory.Date,
                ReceiptUrls = expenseHistory.Receipts.Select(r => r.FileUrl).ToList()
            };
        }

        public async Task<List<ExpenseHistoryDto>> GetExpenseHistoryAsync(int employeeId, DateTime? date = null)
        {
            var histories = await _context.ExpenseHistories
                .Include(eh => eh.Receipts)
                .Where(eh => eh.EmployeeId == employeeId)
                .ToListAsync();

            if (date.HasValue)
            {
                histories = histories.Where(h => h.Date.Date == date.Value.Date).ToList();
            }

            return histories.Select(h => new ExpenseHistoryDto
            {
                Id = h.Id,
                Amount = h.Amount,
                Date = h.Date,
                ReceiptUrls = h.Receipts.Select(r => r.FileUrl).ToList()
            }).ToList();
        }
    }
}