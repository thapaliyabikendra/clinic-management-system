using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace ClinicManagementSystem.Students;

public interface IStudentRepository : IRepository<Student, Guid>
{
    Task<Student?> FindByEmailAsync(
        string email,
        Guid? excludeStudentId = null,
        CancellationToken cancellationToken = default);
}
