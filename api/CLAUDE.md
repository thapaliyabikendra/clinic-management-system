# CLAUDE.md - Clinic Management System API

## Project Overview

- **ABP Framework**: v10.0.1 (Open Source)
- **UI Type**: Angular (separate `angular/` folder) + AuthServer (Razor Pages)
- **Template**: Layered DDD startup template

## Technical Stack

- **.NET**: 10.0 | **EF Core Provider**: PostgreSQL
- **Auth**: OpenIddict (OAuth 2.0) | **Caching**: Redis
- **Object Mapping**: Mapperly (NOT AutoMapper)
- **Testing**: xUnit with `AbpIntegratedTest<TModule>`

## Build & Test Commands

```bash
dotnet build ClinicManagementSystem.slnx              # Build solution
dotnet run --project src/ClinicManagementSystem.DbMigrator  # Run migrations
dotnet test ClinicManagementSystem.slnx               # Run all tests
dotnet test test/ClinicManagementSystem.EntityFrameworkCore.Tests  # Integration tests
dotnet test test/ClinicManagementSystem.Domain.Tests  # Domain unit tests
dotnet run --project src/ClinicManagementSystem.HttpApi.Host  # Run API
dotnet run --project src/ClinicManagementSystem.AuthServer    # Run AuthServer
```

## ABP-Specific Naming Rules

| Type | Convention | Example |
|------|------------|---------|
| Application Service | `*AppService` | `StudentAppService` |
| App Service Interface | `I*AppService` | `IStudentAppService` |
| DTO | `*Dto` | `StudentDto`, `CreateStudentDto` |
| Entity | PascalCase (no suffix) | `Student`, `Appointment` |
| Repository Interface | `I*Repository` | `IStudentRepository` |
| Domain Service | `*Manager` | `StudentManager` |
| Specification | `*Specification` | `ActiveStudentsSpecification` |
| Permission | `*Permissions` (static) | `ClinicManagementSystemPermissions.Students.Create` |
| Mapper | `*Mappers` (static partial) | `StudentMappers` |

## Architecture Map

```
src/
├── *.Domain.Shared       # [DOMAIN] Constants, Enums, Localization
├── *.Domain              # [DOMAIN] Entities, Repository interfaces, Domain Services
├── *.Application.Contracts   # [APPLICATION] DTOs, App Service Interfaces, Permissions
├── *.Application         # [APPLICATION] App Service Implementations, Mapperly Mappers
├── *.EntityFrameworkCore # [INFRASTRUCTURE] DbContext, Repositories, Migrations
├── *.HttpApi             # [PRESENTATION] Controllers (optional - ABP auto-generates)
├── *.HttpApi.Host        # [PRESENTATION] API Host Configuration
├── *.HttpApi.Client      # [CLIENT] Dynamic API Proxies
├── *.AuthServer          # [PRESENTATION] OAuth Server
└── *.DbMigrator          # [TOOLING] Migration Runner

test/
├── *.TestBase            # Shared test infrastructure
├── *.Domain.Tests        # Pure unit tests (no DI, no database)
├── *.Application.Tests   # Reserved for application-only tests
└── *.EntityFrameworkCore.Tests  # Integration tests (USE THIS for most tests)
```

## Speckit Workflow (Spec-Driven Development)

Feature specifications are stored in `specs/{feature-id}/`:

```
specs/
└── 001-student-management/
    ├── spec.md           # User stories, requirements, acceptance criteria
    ├── plan.md           # Technical design, architecture decisions
    ├── data-model.md     # Entity definitions, relationships
    ├── contracts/        # API endpoint contracts
    ├── research.md       # Technical decisions, alternatives considered
    ├── quickstart.md     # Validation scenarios, test checklist
    └── tasks.md          # Implementation tasks (generated)
```

**Speckit Commands:**
- `/speckit.specify` - Create feature specification from description
- `/speckit.plan` - Generate implementation plan
- `/speckit.tasks` - Generate implementation tasks
- `/speckit.implement` - Execute implementation
- `/speckit.analyze` - Cross-artifact consistency check

## Object Mapping (Mapperly)

This project uses **Mapperly** (source generator), NOT AutoMapper.

**Mapper Pattern:**
```csharp
// Location: src/*.Application/{Feature}/{Feature}Mappers.cs
[Mapper]
public static partial class StudentMappers
{
    [MapperIgnoreSource(nameof(Student.TenantId))]
    [MapperIgnoreSource(nameof(Student.IsDeleted))]
    [MapperIgnoreSource(nameof(Student.DeleterId))]
    [MapperIgnoreSource(nameof(Student.DeletionTime))]
    [MapperIgnoreSource(nameof(Student.ExtraProperties))]
    [MapperIgnoreSource(nameof(Student.ConcurrencyStamp))]
    public static partial StudentDto ToDto(this Student student);
}

// Usage in AppService:
return student.ToDto();  // Extension method pattern
```

**Common Ignored Properties (ABP audit fields):**
- `TenantId`, `IsDeleted`, `DeleterId`, `DeletionTime`
- `CreatorId`, `LastModifierId` (if not exposing)
- `ExtraProperties`, `ConcurrencyStamp`

## Test Infrastructure

### Integration Tests (EntityFrameworkCore.Tests)

**IMPORTANT**: Use `ClinicManagementSystemEntityFrameworkCoreTestBase` for integration tests:

```csharp
[Collection(ClinicManagementSystemTestConsts.CollectionDefinitionName)]
public class StudentAppServiceTests : ClinicManagementSystemEntityFrameworkCoreTestBase
{
    private readonly IStudentAppService _studentAppService;

    public StudentAppServiceTests()
    {
        _studentAppService = GetRequiredService<IStudentAppService>();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsStudentDto()
    {
        // Arrange, Act, Assert
    }
}
```

**Key Points:**
- `[Collection(...)]` attribute REQUIRED for SQLite shared context
- Use `GetRequiredService<T>()` to resolve services
- Tests run against SQLite in-memory (not PostgreSQL)
- True concurrency NOT supported (use sequential operations for load tests)

### Domain Unit Tests (Domain.Tests)

For pure unit tests without DI:

```csharp
public class StudentManagerTests
{
    [Fact]
    public void CalculateAge_ShouldReturnCorrectAge()
    {
        // Pure unit test - no DI required
        var age = StudentManager.CalculateAge(
            new DateTime(2000, 1, 15),
            new DateTime(2026, 1, 27));
        age.ShouldBe(26);
    }
}
```

### Test Naming Convention

`MethodName_Scenario_ExpectedResult`

Examples:
- `CreateAsync_WithValidData_ReturnsStudentDto`
- `CreateAsync_WithUnder18DateOfBirth_ThrowsBusinessException`
- `GetListAsync_WithNameFilter_ReturnsMatchingStudents`

## Common Code Patterns

### Entity Creation

```csharp
public class Student : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public string FirstName { get; private set; } = null!;

    protected Student() { } // EF Core constructor

    public Student(Guid id, string firstName, ...) : base(id)
    {
        SetFirstName(firstName);
    }

    internal Student SetFirstName(string firstName)
    {
        FirstName = Check.NotNullOrWhiteSpace(firstName, nameof(firstName), StudentConsts.MaxFirstNameLength);
        return this;
    }
}
```

### Domain Service (Manager)

```csharp
public class StudentManager : DomainService
{
    private readonly IStudentRepository _studentRepository;

    public StudentManager(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Student> CreateAsync(string firstName, ...)
    {
        ValidateAge(dateOfBirth);
        await CheckEmailUniquenessAsync(email);

        return new Student(GuidGenerator.Create(), firstName, ...);
    }

    public void ValidateAge(DateTime dateOfBirth)
    {
        var age = CalculateAge(dateOfBirth, Clock.Now);
        if (age < StudentConsts.MinimumAge)
        {
            throw new BusinessException("ClinicManagement:Student:MustBe18OrOlder")
                .WithData("MinimumAge", StudentConsts.MinimumAge);
        }
    }
}
```

### Business Exception

```csharp
// Throw with localized message
throw new BusinessException("ClinicManagement:Student:DuplicateEmail")
    .WithData("Email", email);

// Localization key in en.json:
"ClinicManagement:Student:DuplicateEmail": "A student with email '{Email}' already exists."
```

### Permission Definition

```csharp
// In Permissions class
public static class Students
{
    public const string Default = GroupName + ".Students";
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
}

// In PermissionDefinitionProvider
var studentsPermission = myGroup.AddPermission(ClinicManagementSystemPermissions.Students.Default, L("Permission:Students"));
studentsPermission.AddChild(ClinicManagementSystemPermissions.Students.Create, L("Permission:Students.Create"));
```

## Non-Obvious ABP CLI Commands

```bash
# Add ABP module
abp add-module Volo.Abp.BlobStoring

# Generate Angular proxies after API changes
abp generate-proxy -t ng -u https://localhost:44324

# Install frontend libs (run after clone)
abp install-libs

# Add EF migration
dotnet ef migrations add <Name> -p src/ClinicManagementSystem.EntityFrameworkCore -s src/ClinicManagementSystem.HttpApi.Host

# Update database directly
dotnet ef database update -p src/ClinicManagementSystem.EntityFrameworkCore -s src/ClinicManagementSystem.HttpApi.Host
```

## Key Files

- **DbContext**: `src/*.EntityFrameworkCore/EntityFrameworkCore/ClinicManagementSystemDbContext.cs`
- **Permissions**: `src/*.Application.Contracts/Permissions/`
- **Localization**: `src/*.Domain.Shared/Localization/`
- **API Config**: `src/*.HttpApi.Host/appsettings.json`
- **Constitution**: `.specify/memory/constitution.md`
- **Feature Specs**: `../specs/{feature-id}/`

## New Feature Implementation Checklist

1. **Setup Phase**
   - [ ] Create feature folders in all layers (`Domain`, `Domain.Shared`, `Application.Contracts`, `Application`, `EntityFrameworkCore`)
   - [ ] Add constants class (`{Feature}Consts.cs`) in Domain.Shared
   - [ ] Add localization keys to `en.json`

2. **Domain Phase**
   - [ ] Create entity with `FullAuditedAggregateRoot<Guid>` and `IMultiTenant`
   - [ ] Create repository interface (`I{Feature}Repository`)
   - [ ] Create domain service (`{Feature}Manager`) with validation logic

3. **Infrastructure Phase**
   - [ ] Create EF Core configuration (`{Feature}EfCoreConfiguration`)
   - [ ] Create repository implementation (`{Feature}Repository`)
   - [ ] Register entity in DbContext (`DbSet<>` and `ApplyConfiguration`)
   - [ ] Add EF migration

4. **Application Phase**
   - [ ] Create DTOs in Application.Contracts
   - [ ] Create app service interface (`I{Feature}AppService`)
   - [ ] Create Mapperly mapper (`{Feature}Mappers`)
   - [ ] Create app service (`{Feature}AppService`)
   - [ ] Define permissions and register in provider

5. **Testing Phase**
   - [ ] Create integration tests in EntityFrameworkCore.Tests
   - [ ] Create domain unit tests in Domain.Tests (if complex logic)
   - [ ] Run all tests: `dotnet test --filter "FullyQualifiedName~{Feature}"`

6. **Documentation Phase**
   - [ ] Update api-technical-documentation.md
   - [ ] Update quickstart.md verification checklist
