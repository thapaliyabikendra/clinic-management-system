using System;
using System.ComponentModel.DataAnnotations;

namespace ClinicManagementSystem.Students;

public class UpdateStudentDto
{
    [Required]
    [StringLength(StudentConsts.MaxFirstNameLength)]
    public string FirstName { get; set; } = null!;

    [Required]
    [StringLength(StudentConsts.MaxLastNameLength)]
    public string LastName { get; set; } = null!;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [EmailAddress]
    [StringLength(StudentConsts.MaxEmailLength)]
    public string? Email { get; set; }

    [StringLength(StudentConsts.MaxPhoneNumberLength)]
    public string? PhoneNumber { get; set; }

    [StringLength(StudentConsts.MaxAddressLength)]
    public string? Address { get; set; }
}
