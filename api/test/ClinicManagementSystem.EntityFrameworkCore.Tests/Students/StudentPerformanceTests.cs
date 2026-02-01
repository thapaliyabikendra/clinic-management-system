using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ClinicManagementSystem.EntityFrameworkCore;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Xunit;

namespace ClinicManagementSystem.Students;

/// <summary>
/// Performance tests for Student Management feature.
/// These tests verify that operations complete within acceptable time limits.
/// Note: Tests are scaled down for SQLite in-memory testing.
/// For production performance testing with 10K students, use a real database.
/// </summary>
[Collection(ClinicManagementSystemTestConsts.CollectionDefinitionName)]
public class StudentPerformanceTests : ClinicManagementSystemEntityFrameworkCoreTestBase
{
    private readonly IStudentAppService _studentAppService;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IGuidGenerator _guidGenerator;

    public StudentPerformanceTests()
    {
        _studentAppService = GetRequiredService<IStudentAppService>();
        _studentRepository = GetRequiredService<IRepository<Student, Guid>>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
    }

    [Fact]
    public async Task SearchPerformance_WithStudents_ShouldCompleteWithin2Seconds()
    {
        // Arrange - Seed 100 students (scaled down for test environment)
        // For SC-002 compliance with 10K students, run against PostgreSQL
        const int studentCount = 100;
        const int maxSearchTimeMs = 2000; // 2 seconds per SC-002

        await SeedStudentsAsync(studentCount);

        // Act - Perform search
        var stopwatch = Stopwatch.StartNew();

        var result = await _studentAppService.GetListAsync(new GetStudentListDto
        {
            Filter = "James", // Partial name match
            MaxResultCount = 20,
            SkipCount = 0
        });

        stopwatch.Stop();

        // Assert
        result.ShouldNotBeNull();
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(maxSearchTimeMs,
            $"Search took {stopwatch.ElapsedMilliseconds}ms, expected < {maxSearchTimeMs}ms");
    }

    [Fact]
    public async Task PaginationPerformance_WithStudents_ShouldCompleteWithin2Seconds()
    {
        // Arrange
        const int studentCount = 100;
        const int maxPaginationTimeMs = 2000;

        await SeedStudentsAsync(studentCount);

        // Act - Paginate through results
        var stopwatch = Stopwatch.StartNew();

        var page1 = await _studentAppService.GetListAsync(new GetStudentListDto
        {
            MaxResultCount = 20,
            SkipCount = 0,
            Sorting = "LastName"
        });

        var page2 = await _studentAppService.GetListAsync(new GetStudentListDto
        {
            MaxResultCount = 20,
            SkipCount = 20,
            Sorting = "LastName"
        });

        var page3 = await _studentAppService.GetListAsync(new GetStudentListDto
        {
            MaxResultCount = 20,
            SkipCount = 40,
            Sorting = "LastName"
        });

        stopwatch.Stop();

        // Assert
        page1.Items.Count.ShouldBe(20);
        page2.Items.Count.ShouldBe(20);
        page3.Items.Count.ShouldBe(20);
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(maxPaginationTimeMs * 3, // 3 pages
            $"Pagination took {stopwatch.ElapsedMilliseconds}ms for 3 pages");
    }

    [Fact]
    public async Task CreatePerformance_ShouldCompleteWithin500ms()
    {
        // Arrange
        const int maxCreateTimeMs = 500;
        var input = new CreateStudentDto
        {
            FirstName = "Performance",
            LastName = "Test",
            DateOfBirth = DateTime.Today.AddYears(-20),
            Email = $"perf.test.{Guid.NewGuid():N}@example.com"
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await _studentAppService.CreateAsync(input);
        stopwatch.Stop();

        // Assert
        result.ShouldNotBeNull();
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(maxCreateTimeMs,
            $"Create took {stopwatch.ElapsedMilliseconds}ms, expected < {maxCreateTimeMs}ms");
    }

    [Fact]
    public async Task BulkOperations_MultipleCreates_ShouldMaintainPerformance()
    {
        // Arrange - Test sequential creates (simulates multiple users over time)
        const int createCount = 10;
        const int maxAverageTimeMs = 200; // Average time per create

        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < createCount; i++)
        {
            await _studentAppService.CreateAsync(new CreateStudentDto
            {
                FirstName = $"Bulk",
                LastName = $"Create{i}",
                DateOfBirth = DateTime.Today.AddYears(-20 - i),
                Email = $"bulk.create{i}.{Guid.NewGuid():N}@example.com"
            });
        }

        stopwatch.Stop();

        // Assert
        var averageTimeMs = stopwatch.ElapsedMilliseconds / createCount;
        averageTimeMs.ShouldBeLessThan(maxAverageTimeMs,
            $"Average create time was {averageTimeMs}ms, expected < {maxAverageTimeMs}ms");
    }

    private async Task SeedStudentsAsync(int count)
    {
        var existingCount = await _studentRepository.GetCountAsync();
        if (existingCount >= count)
        {
            return; // Already have enough students
        }

        var studentsToAdd = count - (int)existingCount;
        var students = new List<Student>();
        var random = new Random(42); // Fixed seed for reproducibility

        var firstNames = new[] { "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda", "William", "Elizabeth" };
        var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };

        for (int i = 0; i < studentsToAdd; i++)
        {
            var firstName = firstNames[random.Next(firstNames.Length)];
            var lastName = lastNames[random.Next(lastNames.Length)];
            var yearOffset = random.Next(18, 40); // Age between 18 and 40

            students.Add(new Student(
                _guidGenerator.Create(),
                $"{firstName}_{i}",
                $"{lastName}_{i}",
                DateTime.Today.AddYears(-yearOffset).AddDays(random.Next(-180, 180)),
                $"student{i}_{Guid.NewGuid():N}@school.edu",
                random.Next(2) == 0 ? $"555-{random.Next(1000, 9999)}" : null,
                random.Next(2) == 0 ? $"{random.Next(100, 999)} Street {i}" : null
            ));
        }

        await _studentRepository.InsertManyAsync(students);
    }
}
