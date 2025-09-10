using HRManagementSystem.Domain.Enums;
using HRManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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

        public LeaveJobService(HRDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Süresi geçen ve hâlâ "İzinli" görünen izinleri ve çalışan statülerini otomatik günceller.
        /// Bu methodu Hangfire ile periyodik olarak çağırabilirsin.
        /// </summary>
        public async Task AutoUpdateEmployeeStatusAsync()
        {
            var today = DateTime.UtcNow.Date;

            // Süresi bitmiş ve hâlâ izinli olan izinleri çek
            var expiredLeaves = await _context.Leaves
                .Where(l => l.Status == LeaveStatus.Izinli && l.EndDate < today)
                .ToListAsync();

            foreach (var leave in expiredLeaves)
            {
                // İlgili çalışanı getir
                var employee = await _context.Employees.FindAsync(leave.EmployeeId);
                if (employee != null && employee.WorkingStatus != "Çalışıyor")
                {
                    employee.WorkingStatus = "Çalışıyor";
                }

                // İzin statüsünü güncelle
                leave.Status = LeaveStatus.Tamamlandi;
            }

            await _context.SaveChangesAsync();
        }
    }
}
