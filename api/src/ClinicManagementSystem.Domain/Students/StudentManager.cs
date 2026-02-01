using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace ClinicManagementSystem.Students;

public class StudentManager : DomainService
{
    private readonly IStudentRepository _studentRepository;

    public StudentManager(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Student> CreateAsync(
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        string? email = null,
        string? phoneNumber = null,
        string? address = null)
    {
        ValidateAge(dateOfBirth);

        if (!string.IsNullOrWhiteSpace(email))
        {
            await CheckEmailUniquenessAsync(email);
        }

        return new Student(
            GuidGenerator.Create(),
            firstName,
            lastName,
            dateOfBirth,
            email,
            phoneNumber,
            address);
    }

    public async Task UpdateAsync(
        Student student,
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        string? email = null,
        string? phoneNumber = null,
        string? address = null)
    {
        ValidateAge(dateOfBirth);

        if (!string.IsNullOrWhiteSpace(email))
        {
            await CheckEmailUniquenessAsync(email, student.Id);
        }

        student.SetFirstName(firstName);
        student.SetLastName(lastName);
        student.SetDateOfBirth(dateOfBirth);
        student.SetEmail(email);
        student.SetPhoneNumber(phoneNumber);
        student.SetAddress(address);
    }

    public void ValidateAge(DateTime dateOfBirth)
    {
        var age = CalculateAge(dateOfBirth, Clock.Now);
        if (age < StudentConsts.MinimumAge)
        {
            throw new BusinessException("ClinicManagement:Student:MustBe18OrOlder")
                .WithData("MinimumAge", StudentConsts.MinimumAge);
        }
    }

    public static int CalculateAge(DateTime dateOfBirth, DateTime referenceDate)
    {
        var age = referenceDate.Year - dateOfBirth.Year;
        if (referenceDate.Date < dateOfBirth.AddYears(age))
        {
            age--;
        }
        return age;
    }

    private async Task CheckEmailUniquenessAsync(string email, Guid? excludeStudentId = null)
    {
        var existingStudent = await _studentRepository.FindByEmailAsync(email, excludeStudentId);
        if (existingStudent != null)
        {
            throw new BusinessException("ClinicManagement:Student:DuplicateEmail")
                .WithData("Email", email);
        }
    }
}
