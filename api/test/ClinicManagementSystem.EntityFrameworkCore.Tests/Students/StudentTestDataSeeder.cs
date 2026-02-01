using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace ClinicManagementSystem.Students;

public class StudentTestDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IGuidGenerator _guidGenerator;

    public StudentTestDataSeeder(
        IRepository<Student, Guid> studentRepository,
        IGuidGenerator guidGenerator)
    {
        _studentRepository = studentRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Only seed if no students exist
        if (await _studentRepository.GetCountAsync() > 0)
        {
            return;
        }

        // Seed sample students for testing
        var students = new[]
        {
            new Student(
                _guidGenerator.Create(),
                "John",
                "Doe",
                new DateTime(2000, 1, 15), // 26 years old
                "john.doe@school.edu",
                "555-0101",
                "123 Main St, City"),

            new Student(
                _guidGenerator.Create(),
                "Jane",
                "Smith",
                new DateTime(1998, 6, 20), // 28 years old
                "jane.smith@school.edu",
                "555-0102",
                "456 Oak Ave, Town"),

            new Student(
                _guidGenerator.Create(),
                "Alice",
                "Johnson",
                new DateTime(2002, 3, 10), // 24 years old
                "alice.johnson@school.edu",
                "555-0103",
                "789 Pine Rd, Village"),

            new Student(
                _guidGenerator.Create(),
                "Bob",
                "Williams",
                new DateTime(2005, 11, 5), // 21 years old
                "bob.williams@school.edu",
                null, // No phone
                null), // No address

            new Student(
                _guidGenerator.Create(),
                "Charlie",
                "Brown",
                new DateTime(2004, 8, 25), // 22 years old
                "charlie.brown@school.edu",
                "555-0105",
                "321 Elm St, Borough")
        };

        await _studentRepository.InsertManyAsync(students);
    }
}
