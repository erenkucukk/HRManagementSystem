using HRManagementSystem.Application.Common;
using HRManagementSystem.Application.DTOs;
using HRManagementSystem.Application.Employees.DTOs;
using HRManagementSystem.Domain.Entities;
using HRManagementSystem.Infrastructure.Persistence;
using HRManagementSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
                    DepartmentName = e.Department?.Name ?? string.Empty
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
                DepartmentName = employee.Department?.Name ?? string.Empty
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
                DepartmentId = dto.DepartmentId
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
    }
}
