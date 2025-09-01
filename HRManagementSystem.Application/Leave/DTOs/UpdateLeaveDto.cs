using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.Application.Leave.DTOs
{
    public class UpdateLeaveDto
    {
        public string Status { get; set; } = string.Empty; // Approved / Rejected
    }
}
