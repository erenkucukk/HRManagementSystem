using HRManagementSystem.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.Application.Services
{
    public class LeaveJobService
    {
        private readonly HRDbContext _context;
        public LeaveJobService(HRDbContext context) => _context = context;

        public async Task AutoUpdateEmployeeStatusAsync()
        {
            var today = DateTime.UtcNow.Date;
            var expiredLeaves = _context.Leaves
                .Where(l => l.Status == "İzinli" && l.EndDate < today)
                .ToList();

            foreach (var leave in expiredLeaves)
            {
                var employee = _context.Employees.Find(leave.EmployeeId);
                if (employee != null && employee.WorkingStatus != "Çalışıyor")
                    employee.WorkingStatus = "Çalışıyor";

                leave.Status = "Tamamlandı";
            }

            await _context.SaveChangesAsync();
        }
    }
}
