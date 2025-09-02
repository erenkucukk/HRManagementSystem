using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.Domain.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TCKimlik { get; set; }
        public DateTime DogumTarihi { get; set; }
        public string TelNo { get; set; }
        public string Email { get; set; }
        public string Position { get; set; }
        public string WorkingStatus { get; set; }
        public string PersonnelPhoto { get; set; }
        public DateTime StartDate { get; set; }
        public int TotalLeave { get; set; }
        public int UsedLeave { get; set; }
        public int DepartmentId { get; set; }
        public Department Department { get; set; }
    }
}


