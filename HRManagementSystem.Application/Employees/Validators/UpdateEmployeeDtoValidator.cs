using FluentValidation;
using HRManagementSystem.Application.Employees.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRManagementSystem.Application.Employees.Validators
{
    public class UpdateEmployeeDtoValidator : AbstractValidator<UpdateEmployeeDto>
    {
        public UpdateEmployeeDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Ad alanı boş bırakılamaz.")
                .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Soyad alanı boş bırakılamaz.")
                .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir.");

            RuleFor(x => x.TCKimlik)
                .NotEmpty().WithMessage("T.C. Kimlik numarası girilmelidir.")
                .Length(11).WithMessage("T.C. Kimlik numarası 11 haneli olmalıdır.");

            RuleFor(x => x.DogumTarihi)
                .NotEmpty().WithMessage("Doğum tarihi girilmelidir.")
                .LessThanOrEqualTo(DateTime.Today).WithMessage("Doğum tarihi gelecekte olamaz.");

            RuleFor(x => x.TelNo)
                .MaximumLength(50).WithMessage("Telefon numarası en fazla 50 karakter olabilir.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email alanı boş bırakılamaz.")
                .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.")
                .MaximumLength(200).WithMessage("Email en fazla 200 karakter olabilir.");

            RuleFor(x => x.Position)
                .NotEmpty().WithMessage("Pozisyon alanı boş bırakılamaz.")
                .MaximumLength(100).WithMessage("Pozisyon en fazla 100 karakter olabilir.");

            RuleFor(x => x.WorkingStatus)
                .NotEmpty().WithMessage("Çalışma durumu girilmelidir.")
                .MaximumLength(100).WithMessage("Çalışma durumu en fazla 100 karakter olabilir.");

            RuleFor(x => x.PersonnelPhoto)
                .MaximumLength(500).WithMessage("Fotoğraf yolu en fazla 500 karakter olabilir.");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("İşe başlama tarihi girilmelidir.")
                .LessThanOrEqualTo(DateTime.Today).WithMessage("İşe başlama tarihi gelecekte olamaz.");

            RuleFor(x => x.TotalLeave)
                .GreaterThanOrEqualTo(0).WithMessage("Toplam izin en az 0 olmalıdır.");

            RuleFor(x => x.UsedLeave)
                .GreaterThanOrEqualTo(0).WithMessage("Kullanılan izin en az 0 olmalıdır.")
                .LessThanOrEqualTo(x => x.TotalLeave).WithMessage("Kullanılan izin toplam izinden fazla olamaz.");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("Geçerli bir departman seçilmelidir.");
        }
    }
}
