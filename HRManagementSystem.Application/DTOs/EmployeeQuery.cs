using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.Application.DTOs
{
    public class EmployeeQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 5;

        public int? DepartmentId { get; set; }
        public string? Search { get; set; } // ad, soyad, email’de ara
        public string? SortBy { get; set; } = "HireDate"; // FirstName|LastName|Email|HireDate
        public string? SortDir { get; set; } = "desc";    // asc|desc

    }
}
