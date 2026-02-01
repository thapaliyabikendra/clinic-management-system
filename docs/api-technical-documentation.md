# Clinic Management System - API Technical Documentation

## Overview

The Clinic Management System is a layered web application built using Domain-Driven Design (DDD) principles and the ABP Framework. It provides a complete solution for managing clinic operations.

## Architecture Overview

### Solution Structure
The system consists of the following key components:

1. **ClinicManagementSystem.DbMigrator**: Console application that applies database migrations and seeds initial data
2. **ClinicManagementSystem.AuthServer**: ASP.NET Core MVC/Razor Pages application with OAuth 2.0 integration (OpenIddict) for authentication
3. **ClinicManagementSystem.HttpApi.Host**: ASP.NET Core API application exposing APIs to clients

### Layered Architecture
- **Domain Layer**: Contains core business logic and entities
- **Application Layer**: Contains application services and business logic
- **Infrastructure Layer**: Data access and external service integrations
- **Presentation Layer**: API controllers, UI components

## API Endpoints

### Core API Endpoints
- **Authentication**: `/api/account` - Login, logout, register
- **User Management**: `/api/user` - User CRUD operations
- **Clinic Management**: `/api/clinic` - Clinic data management
- **Patient Management**: `/api/patient` - Patient records
- **Appointment Scheduling**: `/api/appointment` - Appointment booking and management

### Student Management Endpoints

**Base Path**: `/api/app/students`

| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/api/app/students` | Get paginated list of students | `ClinicManagementSystem.Students` |
| GET | `/api/app/students/{id}` | Get student by ID | `ClinicManagementSystem.Students` |
| POST | `/api/app/students` | Create new student | `ClinicManagementSystem.Students.Create` |
| PUT | `/api/app/students/{id}` | Update student | `ClinicManagementSystem.Students.Edit` |
| DELETE | `/api/app/students/{id}` | Soft-delete student | `ClinicManagementSystem.Students.Delete` |

#### Query Parameters (GET List)

| Parameter | Type | Description |
|-----------|------|-------------|
| `Filter` | string | Search by first name or last name (partial match) |
| `Sorting` | string | Sort field and direction (e.g., "LastName", "FirstName desc") |
| `SkipCount` | int | Number of records to skip (pagination) |
| `MaxResultCount` | int | Maximum records to return (default: 10) |

#### Request/Response DTOs

**CreateStudentDto** (POST):
```json
{
  "firstName": "string (required, max 64)",
  "lastName": "string (required, max 64)",
  "dateOfBirth": "date (required, must be 18+ years ago)",
  "email": "string (optional, max 256, unique per tenant)",
  "phoneNumber": "string (optional, max 20)",
  "address": "string (optional, max 512)"
}
```

**UpdateStudentDto** (PUT):
```json
{
  "firstName": "string (required, max 64)",
  "lastName": "string (required, max 64)",
  "dateOfBirth": "date (required, must be 18+ years ago)",
  "email": "string (optional, max 256, unique per tenant)",
  "phoneNumber": "string (optional, max 20)",
  "address": "string (optional, max 512)"
}
```

**StudentDto** (Response):
```json
{
  "id": "guid",
  "firstName": "string",
  "lastName": "string",
  "dateOfBirth": "date",
  "email": "string",
  "phoneNumber": "string",
  "address": "string",
  "age": "int (calculated)",
  "creationTime": "datetime",
  "lastModificationTime": "datetime"
}
```

#### Business Rules

- **Age Validation**: Students must be at least 18 years old (based on date of birth)
- **Email Uniqueness**: Email must be unique within the tenant (school)
- **Multi-Tenant Isolation**: Students are isolated by tenant; cross-tenant access returns 404
- **Soft Delete**: Delete operation marks student as deleted but preserves data for audit

#### Error Codes

| Code | Description |
|------|-------------|
| `ClinicManagement:Student:MustBe18OrOlder` | Date of birth indicates student is under 18 |
| `ClinicManagement:Student:DuplicateEmail` | Email already exists for another student in this tenant |

### API Configuration
- **Base URL**: `https://localhost:44324`
- **Authentication**: JWT Bearer tokens via OpenIddict
- **API Versioning**: v1 endpoints available

## Technology Stack

### Backend Technologies
- **Framework**: ASP.NET Core (.NET 10.0)
- **ABP Framework**: v10.0.1 (Open Source)
- **ORM**: Entity Framework Core
- **Database**: PostgreSQL (configurable)
- **Object Mapping**: Mapperly (source generator)
- **Authentication**: OpenIddict, JWT Bearer tokens
- **Caching**: Redis distributed cache
- **Logging**: Serilog
- **Dependency Injection**: Autofac

### Frontend Technologies
- **Framework**: Angular
- **UI Library**: Angular Material or Bootstrap

## Object Mapping (Mapperly)

This project uses **Mapperly** (compile-time source generator) for object mapping, NOT AutoMapper.

### Mapper Definition Pattern

```csharp
// Location: src/*.Application/{Feature}/{Feature}Mappers.cs
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class StudentMappers
{
    // Ignore ABP audit properties not needed in DTO
    [MapperIgnoreSource(nameof(Student.TenantId))]
    [MapperIgnoreSource(nameof(Student.IsDeleted))]
    [MapperIgnoreSource(nameof(Student.DeleterId))]
    [MapperIgnoreSource(nameof(Student.DeletionTime))]
    [MapperIgnoreSource(nameof(Student.ExtraProperties))]
    [MapperIgnoreSource(nameof(Student.ConcurrencyStamp))]
    public static partial StudentDto ToDto(this Student student);
}
```

### Usage in Application Services

```csharp
public async Task<StudentDto> GetAsync(Guid id)
{
    var student = await _studentRepository.GetAsync(id);
    return student.ToDto();  // Extension method from Mapperly
}

public async Task<PagedResultDto<StudentDto>> GetListAsync(GetStudentListDto input)
{
    var students = await AsyncExecuter.ToListAsync(queryable);
    return new PagedResultDto<StudentDto>(
        totalCount,
        students.Select(s => s.ToDto()).ToList());
}
```

### Common Ignored Properties

When mapping ABP entities to DTOs, typically ignore these audit/system properties:

| Property | Reason |
|----------|--------|
| `TenantId` | Internal multi-tenancy field |
| `IsDeleted` | Soft delete flag (filtered automatically) |
| `DeleterId`, `DeletionTime` | Deletion audit fields |
| `CreatorId`, `LastModifierId` | Usually not exposed (use names instead) |
| `ExtraProperties` | ABP extensibility field |
| `ConcurrencyStamp` | EF Core concurrency token |

## Feature Implementation Patterns

### Standard Feature Structure

```
src/
├── *.Domain.Shared/
│   └── {Feature}/
│       └── {Feature}Consts.cs          # Field length constants
│
├── *.Domain/
│   └── {Feature}/
│       ├── {Feature}.cs                # Entity (FullAuditedAggregateRoot)
│       ├── I{Feature}Repository.cs     # Repository interface
│       └── {Feature}Manager.cs         # Domain service
│
├── *.Application.Contracts/
│   └── {Feature}/
│       ├── {Feature}Dto.cs             # Output DTO
│       ├── Create{Feature}Dto.cs       # Create input DTO
│       ├── Update{Feature}Dto.cs       # Update input DTO
│       ├── Get{Feature}ListDto.cs      # List query DTO
│       └── I{Feature}AppService.cs     # Service interface
│
├── *.Application/
│   └── {Feature}/
│       ├── {Feature}AppService.cs      # Service implementation
│       └── {Feature}Mappers.cs         # Mapperly mapper
│
└── *.EntityFrameworkCore/
    └── {Feature}/
        ├── {Feature}EfCoreConfiguration.cs  # EF Core config
        └── {Feature}Repository.cs           # Repository implementation
```

### Entity Pattern

```csharp
public class Student : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public DateTime DateOfBirth { get; private set; }
    public string? Email { get; private set; }

    protected Student() { } // Required for EF Core

    public Student(Guid id, string firstName, string lastName, DateTime dateOfBirth, string? email)
        : base(id)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        SetDateOfBirth(dateOfBirth);
        SetEmail(email);
    }

    internal Student SetFirstName(string firstName)
    {
        FirstName = Check.NotNullOrWhiteSpace(firstName, nameof(firstName), StudentConsts.MaxFirstNameLength);
        return this;
    }

    // ... other setters
}
```

### Domain Service Pattern

```csharp
public class StudentManager : DomainService
{
    private readonly IStudentRepository _studentRepository;

    public StudentManager(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Student> CreateAsync(string firstName, string lastName, DateTime dateOfBirth, string? email, ...)
    {
        // Domain validation
        ValidateAge(dateOfBirth);
        await CheckEmailUniquenessAsync(email);

        return new Student(GuidGenerator.Create(), firstName, lastName, dateOfBirth, email, ...);
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

### Application Service Pattern

```csharp
[Authorize(ClinicManagementSystemPermissions.Students.Default)]
public class StudentAppService : ApplicationService, IStudentAppService
{
    private readonly IStudentRepository _studentRepository;
    private readonly StudentManager _studentManager;

    public StudentAppService(IStudentRepository studentRepository, StudentManager studentManager)
    {
        _studentRepository = studentRepository;
        _studentManager = studentManager;
    }

    [Authorize(ClinicManagementSystemPermissions.Students.Create)]
    public async Task<StudentDto> CreateAsync(CreateStudentDto input)
    {
        var student = await _studentManager.CreateAsync(
            input.FirstName, input.LastName, input.DateOfBirth, input.Email, ...);

        await _studentRepository.InsertAsync(student);
        return student.ToDto();
    }

    public async Task<PagedResultDto<StudentDto>> GetListAsync(GetStudentListDto input)
    {
        var queryable = await _studentRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var filter = input.Filter.ToLowerInvariant();
            queryable = queryable.Where(s =>
                s.FirstName.ToLower().Contains(filter) ||
                s.LastName.ToLower().Contains(filter));
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        // Apply sorting and pagination...

        var students = await AsyncExecuter.ToListAsync(queryable);
        return new PagedResultDto<StudentDto>(totalCount, students.Select(s => s.ToDto()).ToList());
    }
}
```

### Error Handling Pattern

```csharp
// Define error code constant
public const string DuplicateEmailCode = "ClinicManagement:Student:DuplicateEmail";

// Throw business exception with data
throw new BusinessException(DuplicateEmailCode)
    .WithData("Email", email);

// Add localization in en.json
{
  "ClinicManagement:Student:DuplicateEmail": "A student with email '{Email}' already exists in this school."
}
```

## Spec-Driven Development

Feature specifications are maintained in `specs/{feature-id}/`:

| File | Purpose |
|------|---------|
| `spec.md` | User stories, requirements, acceptance criteria |
| `plan.md` | Technical design, architecture decisions |
| `data-model.md` | Entity definitions, relationships |
| `contracts/` | API endpoint contracts |
| `research.md` | Technical decisions, alternatives considered |
| `quickstart.md` | Validation scenarios, test checklist |
| `tasks.md` | Implementation tasks (generated) |

### Speckit Workflow Commands

| Command | Description |
|---------|-------------|
| `/speckit.specify` | Create feature specification from description |
| `/speckit.clarify` | Ask clarification questions about the spec |
| `/speckit.plan` | Generate implementation plan |
| `/speckit.tasks` | Generate implementation tasks |
| `/speckit.implement` | Execute implementation |
| `/speckit.analyze` | Cross-artifact consistency check |

## Configuration

### Connection Strings
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=ClinicManagementSystem;User ID=root;Password=myPassword;"
  }
}
```

### Authentication Settings
- **Authority**: `https://localhost:44324`
- **RequireHttpsMetadata**: true

## Authentication Flow

1. **User Registration**: POST to `/api/account/register`
2. **User Login**: POST to `/api/account/login`
3. **Token Issuance**: JWT token returned on successful authentication
4. **API Access**: Include Authorization header with Bearer token

## Deployment

### Prerequisites
- .NET 10.0 SDK
- Node.js v20.11+
- Redis server
- PostgreSQL database

### Setup Steps
1. Generate signing certificate using `dotnet dev-certs https`
2. Run database migrations with `ClinicManagementSystem.DbMigrator`
3. Install client-side libraries: `abp install-libs`
4. Start the API host

## Development Setup

### Environment Variables
- `ConnectionStrings__Default`: Database connection string
- `Redis__Configuration`: Redis server address
- `AuthServer__Authority`: Authentication server URL

## API Documentation

The API is documented using Swagger/OpenAPI specification:
- **Swagger UI**: `https://localhost:44324/swagger`
- **OpenAPI JSON**: `https://localhost:44324/swagger/v1/swagger.json`

## Troubleshooting

### Common Issues
1. **Database Connection**: Ensure PostgreSQL is running and accessible
2. **Redis Connection**: Verify Redis server is running
3. **Certificate Issues**: Regenerate signing certificate using `dotnet dev-certs https`
4. **Mapperly Warnings**: Add `[MapperIgnoreSource]` attributes for unmapped properties
5. **Test Failures**: Ensure using correct test base class (`ClinicManagementSystemEntityFrameworkCoreTestBase`)
