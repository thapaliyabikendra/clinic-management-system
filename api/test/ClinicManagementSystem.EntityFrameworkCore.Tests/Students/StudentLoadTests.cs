using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ClinicManagementSystem.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace ClinicManagementSystem.Students;

/// <summary>
/// Load tests for Student Management feature.
/// These tests verify that the system handles high-volume operations without degradation.
/// Per SC-005: System supports at least 50 concurrent administrators performing student operations.
///
/// Note: These tests use sequential operations to work with SQLite in-memory database.
/// For true concurrent load testing (50 parallel users), run against PostgreSQL.
/// </summary>
[Collection(ClinicManagementSystemTestConsts.CollectionDefinitionName)]
public class StudentLoadTests : ClinicManagementSystemEntityFrameworkCoreTestBase
{
    private readonly IStudentAppService _studentAppService;

    public StudentLoadTests()
    {
        _studentAppService = GetRequiredService<IStudentAppService>();
    }

    [Fact]
    public async Task HighVolumeCreates_ShouldMaintainPerformance()
    {
        // Arrange - Simulate 50 sequential creates (represents 50 users over time)
        const int userCount = 50;
        const int maxAverageTimeMs = 200; // Average time per create should be reasonable

        var stopwatch = Stopwatch.StartNew();

        // Act - Create students sequentially
        for (int i = 0; i < userCount; i++)
        {
            await _studentAppService.CreateAsync(new CreateStudentDto
            {
                FirstName = $"LoadTest",
                LastName = $"User{i}",
                DateOfBirth = DateTime.Today.AddYears(-20 - (i % 20)),
                Email = $"loadtest.user{i}.{Guid.NewGuid():N}@test.com"
            });
        }

        stopwatch.Stop();

        // Assert
        var averageTimeMs = stopwatch.ElapsedMilliseconds / userCount;
        averageTimeMs.ShouldBeLessThan(maxAverageTimeMs,
            $"Average create time was {averageTimeMs}ms, expected < {maxAverageTimeMs}ms");
    }

    [Fact]
    public async Task HighVolumeReads_ShouldMaintainPerformance()
    {
        // Arrange - Create students first
        var createdStudents = new List<StudentDto>();
        for (int i = 0; i < 10; i++)
        {
            var student = await _studentAppService.CreateAsync(new CreateStudentDto
            {
                FirstName = $"ReadLoadTest",
                LastName = $"Student{i}",
                DateOfBirth = DateTime.Today.AddYears(-20 - i),
                Email = $"readloadtest{i}.{Guid.NewGuid():N}@test.com"
            });
            createdStudents.Add(student);
        }

        const int readCount = 50;
        const int maxAverageTimeMs = 100;

        var stopwatch = Stopwatch.StartNew();

        // Act - Read students sequentially
        for (int i = 0; i < readCount; i++)
        {
            var studentId = createdStudents[i % createdStudents.Count].Id;
            await _studentAppService.GetAsync(studentId);
        }

        stopwatch.Stop();

        // Assert
        var averageTimeMs = stopwatch.ElapsedMilliseconds / readCount;
        averageTimeMs.ShouldBeLessThan(maxAverageTimeMs,
            $"Average read time was {averageTimeMs}ms, expected < {maxAverageTimeMs}ms");
    }

    [Fact]
    public async Task HighVolumeSearches_ShouldMaintainPerformance()
    {
        // Arrange - Create students for searching
        for (int i = 0; i < 20; i++)
        {
            await _studentAppService.CreateAsync(new CreateStudentDto
            {
                FirstName = $"SearchLoad",
                LastName = $"Person{i}",
                DateOfBirth = DateTime.Today.AddYears(-20 - i),
                Email = $"searchload{i}.{Guid.NewGuid():N}@test.com"
            });
        }

        const int searchCount = 30;
        const int maxAverageTimeMs = 150;

        var searchTerms = new[] { "SearchLoad", "Person", "Load" };
        var stopwatch = Stopwatch.StartNew();

        // Act - Search sequentially
        for (int i = 0; i < searchCount; i++)
        {
            var searchTerm = searchTerms[i % searchTerms.Length];
            await _studentAppService.GetListAsync(new GetStudentListDto
            {
                Filter = searchTerm,
                MaxResultCount = 10
            });
        }

        stopwatch.Stop();

        // Assert
        var averageTimeMs = stopwatch.ElapsedMilliseconds / searchCount;
        averageTimeMs.ShouldBeLessThan(maxAverageTimeMs,
            $"Average search time was {averageTimeMs}ms, expected < {maxAverageTimeMs}ms");
    }

    [Fact]
    public async Task MixedOperations_SimulatingMultipleAdministrators_ShouldMaintainPerformance()
    {
        // Arrange - Create initial students
        var existingStudents = new List<StudentDto>();
        for (int i = 0; i < 5; i++)
        {
            var student = await _studentAppService.CreateAsync(new CreateStudentDto
            {
                FirstName = $"MixedLoad",
                LastName = $"Test{i}",
                DateOfBirth = DateTime.Today.AddYears(-20 - i),
                Email = $"mixedload{i}.{Guid.NewGuid():N}@test.com"
            });
            existingStudents.Add(student);
        }

        const int operationsPerType = 10;
        const int maxTotalTimeMs = 20000; // 20 seconds for all 40 operations

        var stopwatch = Stopwatch.StartNew();

        // Act - Simulate mixed operations from multiple administrators
        // Creates (10 operations)
        for (int i = 0; i < operationsPerType; i++)
        {
            await _studentAppService.CreateAsync(new CreateStudentDto
            {
                FirstName = $"MixedNew",
                LastName = $"User{i}",
                DateOfBirth = DateTime.Today.AddYears(-25),
                Email = $"mixednew{i}.{Guid.NewGuid():N}@test.com"
            });
        }

        // Reads (10 operations)
        for (int i = 0; i < operationsPerType; i++)
        {
            var studentId = existingStudents[i % existingStudents.Count].Id;
            await _studentAppService.GetAsync(studentId);
        }

        // Updates (10 operations)
        for (int i = 0; i < operationsPerType; i++)
        {
            var student = existingStudents[i % existingStudents.Count];
            await _studentAppService.UpdateAsync(student.Id, new UpdateStudentDto
            {
                FirstName = $"Updated{i}",
                LastName = student.LastName,
                DateOfBirth = student.DateOfBirth,
                Email = $"updated{i}.{Guid.NewGuid():N}@test.com"
            });
        }

        // Searches (10 operations)
        for (int i = 0; i < operationsPerType; i++)
        {
            await _studentAppService.GetListAsync(new GetStudentListDto
            {
                Filter = "Mixed",
                MaxResultCount = 10
            });
        }

        stopwatch.Stop();

        // Assert
        var totalOperations = operationsPerType * 4;
        var averageTimeMs = stopwatch.ElapsedMilliseconds / totalOperations;
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(maxTotalTimeMs,
            $"Mixed operations took {stopwatch.ElapsedMilliseconds}ms ({averageTimeMs}ms avg), expected < {maxTotalTimeMs}ms");
    }
}
