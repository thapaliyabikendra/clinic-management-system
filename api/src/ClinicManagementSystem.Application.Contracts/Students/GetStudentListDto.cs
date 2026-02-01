using Volo.Abp.Application.Dtos;

namespace ClinicManagementSystem.Students;

public class GetStudentListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
