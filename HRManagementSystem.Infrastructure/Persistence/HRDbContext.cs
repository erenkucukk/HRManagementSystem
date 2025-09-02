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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Department - Employee
            modelBuilder.Entity<Department>()
                .HasMany(d => d.Employees)
                .WithOne(e => e.Department)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

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
                e.Property(x => x.WorkingStatus).HasMaxLength(100);
                e.Property(x => x.PersonnelPhoto).HasMaxLength(500);
                e.Property(x => x.StartDate).IsRequired();
                e.Property(x => x.TotalLeave).IsRequired();
                e.Property(x => x.UsedLeave).IsRequired();
                e.HasIndex(x => x.Email).IsUnique();
                e.HasIndex(x => x.TCKimlik).IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
