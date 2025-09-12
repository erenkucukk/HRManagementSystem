using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.Application.Expense.DTOs
{
    public class ExpenseHistoryDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public decimal? MealCost { get; set; }
        public decimal? TransportCost { get; set; }
        public decimal? OtherCost { get; set; }
        public List<string> ReceiptUrls { get; set; }
    }
}
