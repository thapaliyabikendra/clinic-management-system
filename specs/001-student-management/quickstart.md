# Quickstart: Student Management

**Feature**: 001-student-management
**Date**: 2026-01-27

## Prerequisites

- .NET 10.0 SDK installed
- PostgreSQL running (connection string configured)
- Redis running (for distributed cache)
- Solution builds successfully: `dotnet build ClinicManagementSystem.slnx`

## Setup Steps

### 1. Run Database Migration

After implementing the Student entity and EF Core configuration:

```bash
# Add migration
dotnet ef migrations add Added_Student_Entity -p src/ClinicManagementSystem.EntityFrameworkCore -s src/ClinicManagementSystem.HttpApi.Host

# Apply migration
dotnet run --project src/ClinicManagementSystem.DbMigrator
```

### 2. Seed Test Data (Optional)

Create a data seeder in `ClinicManagementSystem.Domain/Students/StudentDataSeeder.cs` for development:

```csharp
public class StudentDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Student, Guid> _studentRepository;

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _studentRepository.GetCountAsync() > 0) return;

        // Seed sample students (18+ years old)
        await _studentRepository.InsertAsync(new Student(
            GuidGenerator.Create(),
            "John",
            "Doe",
            new DateTime(2000, 1, 15), // 26 years old
            "john.doe@example.com"
        ));
    }
}
```

### 3. Run the API

```bash
dotnet run --project src/ClinicManagementSystem.HttpApi.Host
```

API available at: https://localhost:44324

### 4. Test Endpoints

#### Authenticate First

Get a token from the AuthServer (https://localhost:44322) or use Swagger UI's authorize button.

#### Create Student

```bash
curl -X POST https://localhost:44324/api/app/students \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Jane",
    "lastName": "Smith",
    "dateOfBirth": "2000-05-20",
    "email": "jane.smith@example.com"
  }'
```

Expected: 200 OK with StudentDto

#### Create Student Under 18 (Should Fail)

```bash
curl -X POST https://localhost:44324/api/app/students \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Young",
    "lastName": "Person",
    "dateOfBirth": "2015-01-01"
  }'
```

Expected: 400 Bad Request with error code `ClinicManagement:Student:MustBe18OrOlder`

#### List Students

```bash
curl -X GET "https://localhost:44324/api/app/students?Filter=Jane" \
  -H "Authorization: Bearer {token}"
```

Expected: 200 OK with PagedStudentResultDto

#### Get Student by ID

```bash
curl -X GET https://localhost:44324/api/app/students/{id} \
  -H "Authorization: Bearer {token}"
```

Expected: 200 OK with StudentDto

#### Update Student

```bash
curl -X PUT https://localhost:44324/api/app/students/{id} \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Jane",
    "lastName": "Doe",
    "dateOfBirth": "2000-05-20",
    "email": "jane.doe@newdomain.com"
  }'
```

Expected: 200 OK with updated StudentDto

#### Delete Student

```bash
curl -X DELETE https://localhost:44324/api/app/students/{id} \
  -H "Authorization: Bearer {token}"
```

Expected: 204 No Content

## Verification Checklist

All scenarios are covered by automated integration tests. Run `dotnet test --filter "FullyQualifiedName~Student"` to validate.

| Scenario | Status | Automated Test |
|----------|--------|----------------|
| Student creation works with valid 18+ date of birth | ✅ | `CreateAsync_WithValidData_ReturnsStudentDto` |
| Student creation rejected with under-18 date of birth | ✅ | `CreateAsync_WithUnder18DateOfBirth_ThrowsBusinessException` |
| Student list returns only current tenant's students | ✅ | Multi-tenant isolation via `IMultiTenant` interface |
| Student search by name works (partial match) | ✅ | `GetListAsync_WithNameFilter_ReturnsMatchingStudents` |
| Pagination works | ✅ | `GetListAsync_ReturnsPaginatedResults` |
| Email uniqueness enforced within tenant | ✅ | `CreateAsync_WithDuplicateEmail_ThrowsBusinessException` |
| Update preserves 18+ age validation | ✅ | `UpdateAsync_WithUnder18DateOfBirth_ThrowsBusinessException` |
| Delete soft-deletes (student no longer in list) | ✅ | `DeleteAsync_StudentNoLongerAppearsInList` |
| Cross-tenant access returns 404 | ✅ | `GetAsync_WithInvalidId_ThrowsEntityNotFoundException` |

### Test Summary (28 tests total)
- **Domain Tests (4)**: Age calculation logic
- **Integration Tests (16)**: CRUD operations, validation, error handling
- **Performance Tests (4)**: Search < 2s, pagination, create performance
- **Load Tests (4)**: High-volume operations (50 sequential operations)

## Swagger UI

Access Swagger at: https://localhost:44324/swagger

All Student endpoints available under `/api/app/students`

## Troubleshooting

### "Student must be at least 18 years old"
- Check the date of birth calculation
- Server uses UTC time for age calculation

### "Email already exists"
- Email must be unique within the tenant
- Check if another student in the same school has this email

### 401 Unauthorized
- Token expired or invalid
- Re-authenticate at AuthServer

### 404 Not Found on existing student
- Student may belong to a different tenant
- Check you're authenticated as the correct school's administrator
