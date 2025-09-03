using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.Application.Leave.DTOs
{
    public class CreateLeaveDto
    {
        public int EmployeeId { get; set; }
        public string LeaveType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDaeeete { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
