# Data Model: Student Management

**Feature**: 001-student-management
**Date**: 2026-01-27

## Entities

### Student

**Layer**: `ClinicManagementSystem.Domain/Students/Student.cs`
**Base Class**: `FullAuditedAggregateRoot<Guid>` (provides Id, audit fields, soft delete)
**Interfaces**: `IMultiTenant` (provides TenantId for multi-tenant isolation)

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | Guid | PK, auto-generated | Inherited from base |
| TenantId | Guid? | FK to Tenant, indexed | From IMultiTenant |
| FirstName | string | Required, MaxLength(64) | |
| LastName | string | Required, MaxLength(64) | |
| DateOfBirth | DateTime | Required | Used for 18+ validation |
| Email | string | MaxLength(256), email format | Unique per tenant |
| PhoneNumber | string? | MaxLength(20) | Optional |
| Address | string? | MaxLength(512) | Optional |
| CreationTime | DateTime | Auto-set | Inherited from base |
| CreatorId | Guid? | Auto-set | Inherited from base |
| LastModificationTime | DateTime? | Auto-set | Inherited from base |
| LastModifierId | Guid? | Auto-set | Inherited from base |
| IsDeleted | bool | Default false | Inherited from base (soft delete) |
| DeletionTime | DateTime? | Set on delete | Inherited from base |
| DeleterId | Guid? | Set on delete | Inherited from base |

**Business Rules**:
- Student must be 18+ years old (calculated from DateOfBirth at time of create/update)
- Email must be unique within the same tenant (allow duplicates across tenants)
- FirstName, LastName, DateOfBirth are required

## Constants

**Layer**: `ClinicManagementSystem.Domain/Students/StudentConsts.cs`

```csharp
public static class StudentConsts
{
    public const int MaxFirstNameLength = 64;
    public const int MaxLastNameLength = 64;
    public const int MaxEmailLength = 256;
    public const int MaxPhoneNumberLength = 20;
    public const int MaxAddressLength = 512;
    public const int MinimumAge = 18;
}
```

## Database Configuration

**Layer**: `ClinicManagementSystem.EntityFrameworkCore/Students/StudentEfCoreConfiguration.cs`

**Table**: `AppStudents` (ABP convention: App prefix for application entities)

**Indexes**:
| Index Name | Columns | Type | Purpose |
|------------|---------|------|---------|
| IX_AppStudents_TenantId_Email | TenantId, Email | Unique | Email uniqueness per tenant |
| IX_AppStudents_TenantId_FirstName_LastName | TenantId, FirstName, LastName | Non-unique | Search performance |

**Configuration**:
```csharp
builder.ToTable("AppStudents");
builder.ConfigureByConvention(); // ABP conventions

builder.Property(s => s.FirstName)
    .IsRequired()
    .HasMaxLength(StudentConsts.MaxFirstNameLength);

builder.Property(s => s.LastName)
    .IsRequired()
    .HasMaxLength(StudentConsts.MaxLastNameLength);

builder.Property(s => s.Email)
    .HasMaxLength(StudentConsts.MaxEmailLength);

builder.Property(s => s.PhoneNumber)
    .HasMaxLength(StudentConsts.MaxPhoneNumberLength);

builder.Property(s => s.Address)
    .HasMaxLength(StudentConsts.MaxAddressLength);

builder.HasIndex(s => new { s.TenantId, s.Email })
    .IsUnique()
    .HasFilter("[Email] IS NOT NULL"); // Only enforce when email provided

builder.HasIndex(s => new { s.TenantId, s.FirstName, s.LastName });
```

## Relationships

```
School (Tenant)          Student
┌─────────────┐         ┌──────────────┐
│ Id (PK)     │ 1    *  │ Id (PK)      │
│ Name        │─────────│ TenantId (FK)│
│ ...         │         │ FirstName    │
└─────────────┘         │ LastName     │
                        │ DateOfBirth  │
                        │ Email        │
                        │ ...          │
                        └──────────────┘

Cardinality: One School has many Students
Relationship: Students isolated by TenantId (automatic via ABP)
```

## DTOs

### StudentDto (Output)

**Layer**: `ClinicManagementSystem.Application.Contracts/Students/StudentDto.cs`

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | |
| FirstName | string | |
| LastName | string | |
| DateOfBirth | DateTime | |
| Age | int | Calculated property (readonly) |
| Email | string? | |
| PhoneNumber | string? | |
| Address | string? | |
| CreationTime | DateTime | For display purposes |

### CreateStudentDto (Input)

**Layer**: `ClinicManagementSystem.Application.Contracts/Students/CreateStudentDto.cs`

| Field | Type | Validation |
|-------|------|------------|
| FirstName | string | Required, MaxLength(64) |
| LastName | string | Required, MaxLength(64) |
| DateOfBirth | DateTime | Required |
| Email | string? | EmailAddress format, MaxLength(256) |
| PhoneNumber | string? | MaxLength(20) |
| Address | string? | MaxLength(512) |

### UpdateStudentDto (Input)

**Layer**: `ClinicManagementSystem.Application.Contracts/Students/UpdateStudentDto.cs`

Same fields as CreateStudentDto.

### GetStudentListDto (Paged Request)

**Layer**: `ClinicManagementSystem.Application.Contracts/Students/GetStudentListDto.cs`
**Base Class**: `PagedAndSortedResultRequestDto`

| Field | Type | Notes |
|-------|------|-------|
| Filter | string? | Search term for FirstName/LastName |

## State Transitions

```
                  ┌─────────────┐
                  │   Created   │
                  └──────┬──────┘
                         │
            ┌────────────┼────────────┐
            │            │            │
            ▼            ▼            ▼
      ┌──────────┐ ┌──────────┐ ┌──────────┐
      │  Update  │ │   View   │ │  Delete  │
      └────┬─────┘ └──────────┘ └────┬─────┘
           │                         │
           └────────────┬────────────┘
                        ▼
                  ┌──────────────┐
                  │ Soft Deleted │
                  │ (IsDeleted)  │
                  └──────────────┘
```

No complex state machine - Student lifecycle is simple CRUD with soft delete.
