using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.Domain.Entities
{
    public class ExpenseHistory
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public Employee Employee { get; set; }
        public ICollection<ExpenseReceipt> Receipts { get; set; }
    }

    public class ExpenseReceipt
    {
        public int Id { get; set; }
        public int ExpenseHistoryId { get; set; }
        public string FileUrl { get; set; }
        public ExpenseHistory ExpenseHistory { get; set; }
    }
}
