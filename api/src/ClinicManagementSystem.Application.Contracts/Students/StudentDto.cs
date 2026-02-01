using System;
using Volo.Abp.Application.Dtos;

namespace ClinicManagementSystem.Students;

public class StudentDto : EntityDto<Guid>
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime DateOfBirth { get; set; }

    public int Age => CalculateAge();

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public DateTime CreationTime { get; set; }

    private int CalculateAge()
    {
        var today = DateTime.Today;
        var age = today.Year - DateOfBirth.Year;
        if (today.Date < DateOfBirth.AddYears(age))
        {
            age--;
        }
        return age;
    }
}
