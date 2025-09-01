using HRManagementSystem.Application.Departments.DTOs;
using HRManagementSystem.Domain.Entities;
using HRManagementSystem.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRManagementSystem.Application.Services
{
    public class DepartmentService
    {
        private readonly IRepository<Department> _departmentRepository;

        public DepartmentService(IRepository<Department> departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public async Task<List<DepartmentDto>> GetAllAsync()
        {
            var departments = await _departmentRepository.GetAllAsync();
            return departments
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name
                })
                .ToList();
        }

        public async Task<DepartmentDto?> GetByIdAsync(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null) return null;

            return new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name
            };
        }

        public async Task<int> CreateAsync(CreateDepartmentDto dto)
        {
            var department = new Department
            {
                Name = dto.Name
            };
            await _departmentRepository.AddAsync(department);
            await _departmentRepository.SaveChangesAsync();
            return department.Id;
        }

        public async Task<bool> UpdateAsync(int id, UpdateDepartmentDto dto)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null) return false;

            department.Name = dto.Name;
            _departmentRepository.Update(department);
            await _departmentRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null) return false;

            _departmentRepository.Remove(department);
            await _departmentRepository.SaveChangesAsync();
            return true;
        }
    }
}