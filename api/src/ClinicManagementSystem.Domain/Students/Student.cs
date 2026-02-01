using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace ClinicManagementSystem.Students;

public class Student : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string FirstName { get; private set; } = null!;

    public string LastName { get; private set; } = null!;

    public DateTime DateOfBirth { get; private set; }

    public string? Email { get; private set; }

    public string? PhoneNumber { get; private set; }

    public string? Address { get; private set; }

    protected Student()
    {
        // Required for EF Core
    }

    public Student(
        Guid id,
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        string? email = null,
        string? phoneNumber = null,
        string? address = null,
        Guid? tenantId = null)
        : base(id)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        SetDateOfBirth(dateOfBirth);
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        TenantId = tenantId;
    }

    public Student SetFirstName(string firstName)
    {
        FirstName = Check.NotNullOrWhiteSpace(firstName, nameof(firstName), StudentConsts.MaxFirstNameLength);
        return this;
    }

    public Student SetLastName(string lastName)
    {
        LastName = Check.NotNullOrWhiteSpace(lastName, nameof(lastName), StudentConsts.MaxLastNameLength);
        return this;
    }

    public Student SetDateOfBirth(DateTime dateOfBirth)
    {
        DateOfBirth = dateOfBirth;
        return this;
    }

    public Student SetEmail(string? email)
    {
        Email = email?.Length > StudentConsts.MaxEmailLength
            ? throw new ArgumentException($"Email cannot exceed {StudentConsts.MaxEmailLength} characters.")
            : email;
        return this;
    }

    public Student SetPhoneNumber(string? phoneNumber)
    {
        PhoneNumber = phoneNumber?.Length > StudentConsts.MaxPhoneNumberLength
            ? throw new ArgumentException($"Phone number cannot exceed {StudentConsts.MaxPhoneNumberLength} characters.")
            : phoneNumber;
        return this;
    }

    public Student SetAddress(string? address)
    {
        Address = address?.Length > StudentConsts.MaxAddressLength
            ? throw new ArgumentException($"Address cannot exceed {StudentConsts.MaxAddressLength} characters.")
            : address;
        return this;
    }
}
