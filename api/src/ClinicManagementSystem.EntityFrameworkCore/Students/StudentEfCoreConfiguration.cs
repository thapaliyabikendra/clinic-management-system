using ClinicManagementSystem.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ClinicManagementSystem.EntityFrameworkCore.Students;

public class StudentEfCoreConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable(ClinicManagementSystemConsts.DbTablePrefix + "Students", ClinicManagementSystemConsts.DbSchema);

        builder.ConfigureByConvention();

        builder.Property(s => s.FirstName)
            .IsRequired()
            .HasMaxLength(StudentConsts.MaxFirstNameLength);

        builder.Property(s => s.LastName)
            .IsRequired()
            .HasMaxLength(StudentConsts.MaxLastNameLength);

        builder.Property(s => s.Email)
            .HasMaxLength(StudentConsts.MaxEmailLength);

        builder.Property(s => s.PhoneNumber)
            .HasMaxLength(StudentConsts.MaxPhoneNumberLength);

        builder.Property(s => s.Address)
            .HasMaxLength(StudentConsts.MaxAddressLength);

        // Unique index on Email per Tenant (only when Email is not null)
        builder.HasIndex(s => new { s.TenantId, s.Email })
            .IsUnique()
            .HasFilter("\"Email\" IS NOT NULL")
            .HasDatabaseName("IX_AppStudents_TenantId_Email");

        // Index for name search performance
        builder.HasIndex(s => new { s.TenantId, s.FirstName, s.LastName })
            .HasDatabaseName("IX_AppStudents_TenantId_FirstName_LastName");
    }
}
