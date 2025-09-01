using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using HRManagementSystem.Application.Employees.DTOs;
using HRManagementSystem.Application.Departments.DTOs;

namespace HRManagementSystem.Application.Employees.Validators
{
    public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDto>
    {
        public CreateEmployeeDtoValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100).WithMessage("First name is required.");
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100).WithMessage("Last name is required.");
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200).WithMessage("Valid email is required.");
            RuleFor(x => x.PhoneNumber).MaximumLength(50);
            RuleFor(x => x.HireDate).LessThanOrEqualTo(DateTime.UtcNow).WithMessage("HireDate gelecekte olamaz.");
            RuleFor(x => x.DepartmentId).GreaterThan(0).WithMessage("DepartmentId must be valid.");
        }
    }


}
