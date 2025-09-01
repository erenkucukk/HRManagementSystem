using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.Infrastructure.Persistence
{
    public class HRDbContextFactory : IDesignTimeDbContextFactory<HRDbContext>
    {
        public HRDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HRDbContext>();
            // Bağlantı stringini buraya ekle (örnek: localdb)
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=HRManagementSystemDb;Trusted_Connection=True;"); return new HRDbContext(optionsBuilder.Options);
        }
    }
}
