using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using ClinicManagementSystem.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ClinicManagementSystem.Students;

[Authorize(ClinicManagementSystemPermissions.Students.Default)]
public class StudentAppService :
    ApplicationService,
    IStudentAppService
{
    private readonly IStudentRepository _studentRepository;
    private readonly StudentManager _studentManager;

    public StudentAppService(
        IStudentRepository studentRepository,
        StudentManager studentManager)
    {
        _studentRepository = studentRepository;
        _studentManager = studentManager;
    }

    public async Task<StudentDto> GetAsync(Guid id)
    {
        var student = await _studentRepository.GetAsync(id);
        return student.ToDto();
    }

    public async Task<PagedResultDto<StudentDto>> GetListAsync(GetStudentListDto input)
    {
        var queryable = await _studentRepository.GetQueryableAsync();

        // Apply name filter
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var filter = input.Filter.ToLowerInvariant();
            queryable = queryable.Where(s =>
                s.FirstName.ToLower().Contains(filter) ||
                s.LastName.ToLower().Contains(filter));
        }

        // Get total count
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(input.Sorting))
        {
            queryable = queryable.OrderBy(input.Sorting);
        }
        else
        {
            queryable = queryable.OrderBy(s => s.LastName).ThenBy(s => s.FirstName);
        }

        // Apply pagination
        queryable = queryable
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var students = await AsyncExecuter.ToListAsync(queryable);

        return new PagedResultDto<StudentDto>(
            totalCount,
            students.Select(s => s.ToDto()).ToList());
    }

    [Authorize(ClinicManagementSystemPermissions.Students.Create)]
    public async Task<StudentDto> CreateAsync(CreateStudentDto input)
    {
        var student = await _studentManager.CreateAsync(
            input.FirstName,
            input.LastName,
            input.DateOfBirth,
            input.Email,
            input.PhoneNumber,
            input.Address);

        await _studentRepository.InsertAsync(student);

        return student.ToDto();
    }

    [Authorize(ClinicManagementSystemPermissions.Students.Edit)]
    public async Task<StudentDto> UpdateAsync(Guid id, UpdateStudentDto input)
    {
        var student = await _studentRepository.GetAsync(id);

        await _studentManager.UpdateAsync(
            student,
            input.FirstName,
            input.LastName,
            input.DateOfBirth,
            input.Email,
            input.PhoneNumber,
            input.Address);

        await _studentRepository.UpdateAsync(student);

        return student.ToDto();
    }

    [Authorize(ClinicManagementSystemPermissions.Students.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var student = await _studentRepository.GetAsync(id);
        await _studentRepository.DeleteAsync(student);
    }
}
