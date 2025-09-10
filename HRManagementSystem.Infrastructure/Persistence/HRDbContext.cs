using Microsoft.EntityFrameworkCore;
using HRManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.Infrastructure.Persistence
{
    public class HRDbContext : DbContext
    {
        public HRDbContext(DbContextOptions<HRDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<ExpenseHistory> ExpenseHistories { get; set; }
        public DbSet<ExpenseReceipt> ExpenseReceipts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Department - Employee
            modelBuilder.Entity<Department>()
                .HasMany(d => d.Employees)
                .WithOne(e => e.Department)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Leave>()
                .HasOne(l => l.Employee)
                .WithMany(e => e.Leaves)
                .HasForeignKey(l => l.EmployeeId);

            // Employee yeni alanlar & kurallar
            modelBuilder.Entity<Employee>(e =>
            {
                e.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
                e.Property(x => x.LastName).IsRequired().HasMaxLength(100);
                e.Property(x => x.TCKimlik).IsRequired().HasMaxLength(11);
                e.Property(x => x.DogumTarihi).IsRequired();
                e.Property(x => x.TelNo).HasMaxLength(50);
                e.Property(x => x.Email).IsRequired().HasMaxLength(200);
                e.Property(x => x.Position).HasMaxLength(100);
                e.Property(x => x.Adres).HasMaxLength(100);
                e.Property(x => x.WorkingStatus).HasMaxLength(100);
                e.Property(x => x.PersonnelPhoto).HasColumnType("nvarchar(max)");
                e.Property(x => x.StartDate).IsRequired();
                e.Property(x => x.TotalLeave).IsRequired();
                e.Property(x => x.UsedLeave).IsRequired();
                e.HasIndex(x => x.Email).IsUnique();
                e.HasIndex(x => x.TCKimlik).IsUnique();
                e.Property(x => x.Salary).HasDefaultValue(0);
                e.Property(x => x.MealCost).HasDefaultValue(0);
                e.Property(x => x.TransportCost).HasDefaultValue(0);
                e.Property(x => x.OtherCost).HasDefaultValue(0);
            });

            modelBuilder.Entity<ExpenseHistory>()
                .HasOne(eh => eh.Employee)
                .WithMany(e => e.ExpenseHistories)
                .HasForeignKey(eh => eh.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExpenseReceipt>()
                .HasOne(er => er.ExpenseHistory)
                .WithMany(eh => eh.Receipts)
                .HasForeignKey(er => er.ExpenseHistoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Leave>()
                .Property(l => l.Status)
                .HasConversion<string>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
