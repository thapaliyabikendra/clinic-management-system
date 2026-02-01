using System;
using Volo.Abp.Application.Services;

namespace ClinicManagementSystem.Students;

public interface IStudentAppService :
    ICrudAppService<
        StudentDto,
        Guid,
        GetStudentListDto,
        CreateStudentDto,
        UpdateStudentDto>
{
}
