using System;
using System.Threading.Tasks;
using ClinicManagementSystem.EntityFrameworkCore;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Xunit;

namespace ClinicManagementSystem.Students;

[Collection(ClinicManagementSystemTestConsts.CollectionDefinitionName)]
public class StudentAppServiceTests : ClinicManagementSystemEntityFrameworkCoreTestBase
{
    private readonly IStudentAppService _studentAppService;
    private readonly IStudentRepository _studentRepository;

    public StudentAppServiceTests()
    {
        _studentAppService = GetRequiredService<IStudentAppService>();
        _studentRepository = GetRequiredService<IStudentRepository>();
    }

    #region Create Tests (US1)

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsStudentDto()
    {
        // Arrange
        var input = new CreateStudentDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = DateTime.Today.AddYears(-20),
            Email = "jane.smith@example.com",
            PhoneNumber = "555-0100",
            Address = "123 Main St"
        };

        // Act
        var result = await _studentAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.FirstName.ShouldBe(input.FirstName);
        result.LastName.ShouldBe(input.LastName);
        result.DateOfBirth.ShouldBe(input.DateOfBirth);
        result.Email.ShouldBe(input.Email);
        result.PhoneNumber.ShouldBe(input.PhoneNumber);
        result.Address.ShouldBe(input.Address);
        result.Age.ShouldBe(20);
    }

    [Fact]
    public async Task CreateAsync_WithUnder18DateOfBirth_ThrowsBusinessException()
    {
        // Arrange
        var input = new CreateStudentDto
        {
            FirstName = "Young",
            LastName = "Person",
            DateOfBirth = DateTime.Today.AddYears(-17), // Under 18
            Email = "young.person@example.com"
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<BusinessException>(
            async () => await _studentAppService.CreateAsync(input));

        exception.Code.ShouldBe("ClinicManagement:Student:MustBe18OrOlder");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ThrowsBusinessException()
    {
        // Arrange - Create first student
        var firstStudent = new CreateStudentDto
        {
            FirstName = "First",
            LastName = "Student",
            DateOfBirth = DateTime.Today.AddYears(-20),
            Email = "duplicate@example.com"
        };
        await _studentAppService.CreateAsync(firstStudent);

        // Arrange - Try to create second student with same email
        var secondStudent = new CreateStudentDto
        {
            FirstName = "Second",
            LastName = "Student",
            DateOfBirth = DateTime.Today.AddYears(-21),
            Email = "duplicate@example.com" // Same email
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<BusinessException>(
            async () => await _studentAppService.CreateAsync(secondStudent));

        exception.Code.ShouldBe("ClinicManagement:Student:DuplicateEmail");
    }

    [Fact]
    public async Task CreateAsync_WithNullEmail_ShouldSucceed()
    {
        // Arrange - Email is optional
        var input = new CreateStudentDto
        {
            FirstName = "No",
            LastName = "Email",
            DateOfBirth = DateTime.Today.AddYears(-25)
            // No email provided
        };

        // Act
        var result = await _studentAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Email.ShouldBeNull();
    }

    #endregion

    #region Get Tests (US2)

    [Fact]
    public async Task GetAsync_WithValidId_ReturnsStudentDto()
    {
        // Arrange
        var created = await _studentAppService.CreateAsync(new CreateStudentDto
        {
            FirstName = "Get",
            LastName = "Test",
            DateOfBirth = DateTime.Today.AddYears(-22),
            Email = "get.test@example.com"
        });

        // Act
        var result = await _studentAppService.GetAsync(created.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(created.Id);
        result.FirstName.ShouldBe("Get");
        result.LastName.ShouldBe("Test");
    }

    [Fact]
    public async Task GetAsync_WithInvalidId_ThrowsEntityNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            async () => await _studentAppService.GetAsync(invalidId));
    }

    #endregion

    #region GetList Tests (US2)

    [Fact]
    public async Task GetListAsync_ReturnsPaginatedResults()
    {
        // Arrange - Create multiple students
        for (int i = 1; i <= 5; i++)
        {
            await _studentAppService.CreateAsync(new CreateStudentDto
            {
                FirstName = $"Student{i}",
                LastName = $"Test{i}",
                DateOfBirth = DateTime.Today.AddYears(-20 - i),
                Email = $"student{i}@test.com"
            });
        }

        // Act
        var result = await _studentAppService.GetListAsync(new GetStudentListDto
        {
            MaxResultCount = 3,
            SkipCount = 0
        });

        // Assert
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(5);
        result.Items.Count.ShouldBe(3);
    }

    [Fact]
    public async Task GetListAsync_WithNameFilter_ReturnsMatchingStudents()
    {
        // Arrange
        await _studentAppService.CreateAsync(new CreateStudentDto
        {
            FirstName = "Alice",
            LastName = "Johnson",
            DateOfBirth = DateTime.Today.AddYears(-20),
            Email = "alice@test.com"
        });

        await _studentAppService.CreateAsync(new CreateStudentDto
        {
            FirstName = "Bob",
            LastName = "Smith",
            DateOfBirth = DateTime.Today.AddYears(-21),
            Email = "bob@test.com"
        });

        // Act
        var result = await _studentAppService.GetListAsync(new GetStudentListDto
        {
            Filter = "Alice"
        });

        // Assert
        result.Items.ShouldContain(s => s.FirstName == "Alice");
        result.Items.ShouldNotContain(s => s.FirstName == "Bob");
    }

    [Fact]
    public async Task GetListAsync_WithLastNameFilter_ReturnsMatchingStudents()
    {
        // Arrange
        await _studentAppService.CreateAsync(new CreateStudentDto
        {
            FirstName = "Filter",
            LastName = "ByLastName",
            DateOfBirth = DateTime.Today.AddYears(-22),
            Email = "filterlast@test.com"
        });

        // Act
        var result = await _studentAppService.GetListAsync(new GetStudentListDto
        {
            Filter = "ByLastName"
        });

        // Assert
        result.Items.ShouldContain(s => s.LastName == "ByLastName");
    }

    #endregion

    #region Update Tests (US3)

    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsUpdatedStudentDto()
    {
        // Arrange
        var created = await _studentAppService.CreateAsync(new CreateStudentDto
        {
            FirstName = "Original",
            LastName = "Name",
            DateOfBirth = DateTime.Today.AddYears(-20),
            Email = "original@test.com"
        });

        var updateInput = new UpdateStudentDto
        {
            FirstName = "Updated",
            LastName = "Name",
            DateOfBirth = DateTime.Today.AddYears(-20),
            Email = "updated@test.com",
            PhoneNumber = "555-9999"
        };

        // Act
        var result = await _studentAppService.UpdateAsync(created.Id, updateInput);

        // Assert
        result.FirstName.ShouldBe("Updated");
        result.Email.ShouldBe("updated@test.com");
        result.PhoneNumber.ShouldBe("555-9999");
    }

    [Fact]
    public async Task UpdateAsync_WithUnder18DateOfBirth_ThrowsBusinessException()
    {
        // Arrange
        var created = await _studentAppService.CreateAsync(new CreateStudentDto
        {
            FirstName = "Adult",
            LastName = "Person",
            DateOfBirth = DateTime.Today.AddYears(-20),
            Email = "adult@test.com"
        });

        var updateInput = new UpdateStudentDto
        {
            FirstName = "Adult",
            LastName = "Person",
            DateOfBirth = DateTime.Today.AddYears(-17), // Change to under 18
            Email = "adult@test.com"
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<BusinessException>(
            async () => await _studentAppService.UpdateAsync(created.Id, updateInput));

        exception.Code.ShouldBe("ClinicManagement:Student:MustBe18OrOlder");
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateEmail_ThrowsBusinessException()
    {
        // Arrange - Create two students
        var student1 = await _studentAppService.CreateAsync(new CreateStudentDto
        {
            FirstName = "Student",
            LastName = "One",
            DateOfBirth = DateTime.Today.AddYears(-20),
            Email = "student.one@test.com"
        });

        var student2 = await _studentAppService.CreateAsync(new CreateStudentDto
        {
            FirstName = "Student",
            LastName = "Two",
            DateOfBirth = DateTime.Today.AddYears(-21),
            Email = "student.two@test.com"
        });

        // Try to update student2 with student1's email
        var updateInput = new UpdateStudentDto
        {
            FirstName = "Student",
            LastName = "Two",
            DateOfBirth = DateTime.Today.AddYears(-21),
            Email = "student.one@test.com" // Duplicate email
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<BusinessException>(
            async () => await _studentAppService.UpdateAsync(student2.Id, updateInput));

        exception.Code.ShouldBe("ClinicManagement:Student:DuplicateEmail");
    }

    [Fact]
    public async Task UpdateAsync_WithSameEmail_ShouldSucceed()
    {
        // Arrange
        var created = await _studentAppService.CreateAsync(new CreateStudentDto
        {
            FirstName = "Keep",
            LastName = "Email",
            DateOfBirth = DateTime.Today.AddYears(-20),
            Email = "keep.email@test.com"
        });

        var updateInput = new UpdateStudentDto
        {
            FirstName = "Changed",
            LastName = "Name",
            DateOfBirth = DateTime.Today.AddYears(-20),
            Email = "keep.email@test.com" // Same email, should be allowed
        };

        // Act
        var result = await _studentAppService.UpdateAsync(created.Id, updateInput);

        // Assert
        result.FirstName.ShouldBe("Changed");
        result.Email.ShouldBe("keep.email@test.com");
    }

    #endregion

    #region Delete Tests (US4)

    [Fact]
    public async Task DeleteAsync_WithValidId_SoftDeletesStudent()
    {
        // Arrange
        var created = await _studentAppService.CreateAsync(new CreateStudentDto
        {
            FirstName = "To",
            LastName = "Delete",
            DateOfBirth = DateTime.Today.AddYears(-20),
            Email = "to.delete@test.com"
        });

        // Act
        await _studentAppService.DeleteAsync(created.Id);

        // Assert - Should throw when trying to get deleted student
        await Should.ThrowAsync<EntityNotFoundException>(
            async () => await _studentAppService.GetAsync(created.Id));
    }

    [Fact]
    public async Task DeleteAsync_StudentNoLongerAppearsInList()
    {
        // Arrange
        var created = await _studentAppService.CreateAsync(new CreateStudentDto
        {
            FirstName = "WillBeDeleted",
            LastName = "Student",
            DateOfBirth = DateTime.Today.AddYears(-20),
            Email = "willbedeleted@test.com"
        });

        // Act
        await _studentAppService.DeleteAsync(created.Id);

        // Assert
        var list = await _studentAppService.GetListAsync(new GetStudentListDto
        {
            Filter = "WillBeDeleted"
        });

        list.Items.ShouldNotContain(s => s.Id == created.Id);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ThrowsEntityNotFoundException()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(
            async () => await _studentAppService.DeleteAsync(invalidId));
    }

    #endregion
}
