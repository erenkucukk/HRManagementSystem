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
        private readonly HRDbContext _context; // DbContext doğrudan eklendi
        private readonly IRepository<Employee> _employeeRepository;

        public EmployeeService(IRepository<Employee> employeeRepository, HRDbContext context)
        {
            _employeeRepository = employeeRepository;
            _context = context;
        }
        public async Task<PagedResult<EmployeeDto>> GetPagedAsync(EmployeeQuery q)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .AsQueryable();

            if (q.DepartmentId.HasValue)
                query = query.Where(e => e.DepartmentId == q.DepartmentId.Value);

            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var s = q.Search.Trim().ToLower();
                query = query.Where(e =>
                    e.FirstName.ToLower().Contains(s) ||
                    e.LastName.ToLower().Contains(s) ||
                    e.Email.ToLower().Contains(s));
            }

            // Sıralama
            bool desc = string.Equals(q.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
            query = (q.SortBy?.ToLower()) switch
            {
                "firstname" => desc ? query.OrderByDescending(x => x.FirstName) : query.OrderBy(x => x.FirstName),
                "lastname" => desc ? query.OrderByDescending(x => x.LastName) : query.OrderBy(x => x.LastName),
                "email" => desc ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
                "hiredate" or _ => desc ? query.OrderByDescending(x => x.HireDate) : query.OrderBy(x => x.HireDate),
            };

            var total = await query.CountAsync();

            var items = await query
                .Skip((q.Page - 1) * q.PageSize)
                .Take(q.PageSize)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    HireDate = e.HireDate,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = e.Department != null ? e.Department.Name : string.Empty
                })
                .ToListAsync();

            return new PagedResult<EmployeeDto>
            {
                Items = items,
                TotalCount = total,
                PageNumber = q.Page,
                PageSize = q.PageSize
            };
        }

        // Tüm çalışanları departman bilgisiyle getir
        public async Task<List<EmployeeDto>> GetAllAsync()
        {
            var employees = await _context.Employees
                                          .Include(e => e.Department) // Departmanı da çekiyoruz
                                          .ToListAsync();

            return employees
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                    HireDate = e.HireDate,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = e.Department != null ? e.Department.Name : string.Empty
                })
                .ToList();
        }

        // Id'ye göre çalışan getir (departman bilgisiyle birlikte)
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
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                HireDate = employee.HireDate,
                DepartmentId = employee.DepartmentId,
                DepartmentName = employee.Department != null ? employee.Department.Name : string.Empty
            };
        }

        // Yeni çalışan ekle
        public async Task<int> CreateAsync(CreateEmployeeDto dto)
        {
            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                HireDate = dto.HireDate,
                DepartmentId = dto.DepartmentId
            };

            await _employeeRepository.AddAsync(employee);
            await _employeeRepository.SaveChangesAsync();
            return employee.Id;
        }

        // Çalışan güncelle
        public async Task<bool> UpdateAsync(int id, UpdateEmployeeDto dto)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null) return false;

            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.Email = dto.Email;
            employee.PhoneNumber = dto.PhoneNumber;
            employee.HireDate = dto.HireDate;
            employee.DepartmentId = dto.DepartmentId;

            _employeeRepository.Update(employee);
            await _employeeRepository.SaveChangesAsync();
            return true;
        }

        // Çalışan sil
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
