using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClinicManagementSystem.Students;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace ClinicManagementSystem.EntityFrameworkCore.Students;

public class StudentRepository : EfCoreRepository<ClinicManagementSystemDbContext, Student, Guid>, IStudentRepository
{
    public StudentRepository(IDbContextProvider<ClinicManagementSystemDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<Student?> FindByEmailAsync(
        string email,
        Guid? excludeStudentId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Where(s => s.Email == email)
            .WhereIf(excludeStudentId.HasValue, s => s.Id != excludeStudentId!.Value)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }
}
