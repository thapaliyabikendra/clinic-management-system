using System;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ClinicManagementSystem.Students;

/// <summary>
/// Pure unit tests for StudentManager that don't require DI container.
/// Tests involving repositories are in StudentAppServiceTests (integration tests).
/// </summary>
public class StudentManagerTests
{
    [Fact]
    public void CalculateAge_ShouldReturnCorrectAge()
    {
        // Arrange
        var dateOfBirth = new DateTime(2000, 6, 15);
        var referenceDate = new DateTime(2026, 1, 27);

        // Act
        var age = StudentManager.CalculateAge(dateOfBirth, referenceDate);

        // Assert
        age.ShouldBe(25);
    }

    [Fact]
    public void CalculateAge_BeforeBirthday_ShouldReturnCorrectAge()
    {
        // Arrange - reference date is before birthday in the year
        var dateOfBirth = new DateTime(2000, 6, 15);
        var referenceDate = new DateTime(2026, 3, 10);

        // Act
        var age = StudentManager.CalculateAge(dateOfBirth, referenceDate);

        // Assert
        age.ShouldBe(25);
    }

    [Fact]
    public void CalculateAge_OnBirthday_ShouldReturnCorrectAge()
    {
        // Arrange - reference date is exactly on 18th birthday
        var dateOfBirth = new DateTime(2008, 1, 27);
        var referenceDate = new DateTime(2026, 1, 27);

        // Act
        var age = StudentManager.CalculateAge(dateOfBirth, referenceDate);

        // Assert
        age.ShouldBe(18);
    }

    [Fact]
    public void CalculateAge_OneDayBeforeBirthday_ShouldReturnCorrectAge()
    {
        // Arrange - reference date is one day before 18th birthday
        var dateOfBirth = new DateTime(2008, 1, 28);
        var referenceDate = new DateTime(2026, 1, 27);

        // Act
        var age = StudentManager.CalculateAge(dateOfBirth, referenceDate);

        // Assert
        age.ShouldBe(17); // Still 17 until birthday
    }
}
