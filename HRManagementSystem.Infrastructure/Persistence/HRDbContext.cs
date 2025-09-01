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
        public HRDbContext(DbContextOptions<HRDbContext> options): base(options)
        {
        }

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

            // Department.Name unique + max length
            modelBuilder.Entity<Department>()
                .Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Department>()
                .HasIndex(d => d.Name)
                .IsUnique();

            // User.Email unique + max length
            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Employee kolon kuralları
            modelBuilder.Entity<Employee>(e =>
            {
                e.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
                e.Property(x => x.LastName).IsRequired().HasMaxLength(100);
                e.Property(x => x.Email).IsRequired().HasMaxLength(200);
                e.Property(x => x.PhoneNumber).HasMaxLength(50);
                e.HasIndex(x => x.Email).IsUnique(); // aynı email’le ikinci çalışan olmasın
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
