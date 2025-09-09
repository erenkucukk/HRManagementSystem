using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HRManagementSystem.Application.Expense.DTOs
{
    public class UpdateCostDto
    {
        public decimal? Salary { get; set; }
        public decimal? MealCost { get; set; }
        public decimal? TransportCost { get; set; }
        public decimal? OtherCost { get; set; }
        public DateTime ExpenseDate { get; set; }
        public List<IFormFile> Receipts { get; set; }
    }
}
