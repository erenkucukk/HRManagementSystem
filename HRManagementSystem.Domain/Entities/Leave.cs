using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.Domain.Entities
{
    public class Leave
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public string LeaveType { get; set; } = string.Empty; // Yıllık, Hastalık, Mazeret
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        public string Reason { get; set; } = string.Empty; // Açıklama
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    }
}
