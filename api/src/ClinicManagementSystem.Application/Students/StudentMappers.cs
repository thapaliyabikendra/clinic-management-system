using Riok.Mapperly.Abstractions;

namespace ClinicManagementSystem.Students;

[Mapper]
public static partial class StudentMappers
{
    [MapperIgnoreSource(nameof(Student.TenantId))]
    [MapperIgnoreSource(nameof(Student.IsDeleted))]
    [MapperIgnoreSource(nameof(Student.DeleterId))]
    [MapperIgnoreSource(nameof(Student.DeletionTime))]
    [MapperIgnoreSource(nameof(Student.LastModificationTime))]
    [MapperIgnoreSource(nameof(Student.LastModifierId))]
    [MapperIgnoreSource(nameof(Student.CreatorId))]
    [MapperIgnoreSource(nameof(Student.ExtraProperties))]
    [MapperIgnoreSource(nameof(Student.ConcurrencyStamp))]
    public static partial StudentDto ToDto(this Student student);
}
