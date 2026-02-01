# Implementation Plan: Student Management

**Branch**: `001-student-management` | **Date**: 2026-01-27 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-student-management/spec.md`

## Summary

Implement a multi-tenant Student Management feature with full CRUD operations. The feature includes:
- Student entity with age validation (18+ years required)
- Multi-tenant isolation via ABP's `IMultiTenant` interface
- `StudentAppService` using ABP's `CrudAppService<>` pattern
- Paginated listing with name-based search
- Soft delete for audit compliance

## Technical Context

**Language/Version**: C# / .NET 10.0
**Primary Dependencies**: ABP Framework 10.0.1, Entity Framework Core 10.0
**Storage**: PostgreSQL via `Volo.Abp.EntityFrameworkCore.PostgreSql`
**Testing**: xUnit with `AbpIntegratedTest<TModule>`
**Target Platform**: ASP.NET Core Web API
**Project Type**: Layered DDD (ABP template)
**Performance Goals**: p95 < 200ms for CRUD, p95 < 500ms for search with 10K students
**Constraints**: Multi-tenant isolation, 18+ age validation, soft delete
**Scale/Scope**: 50 concurrent administrators, 10,000 students per school

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Requirement | Compliance |
|-----------|-------------|------------|
| I. Strict DDD Layer Adherence | Student entity in Domain, StudentAppService in Application, DTOs in Application.Contracts | ✅ PASS |
| II. ABP Integration Testing | Integration tests for StudentAppService in *.Application.Tests | ✅ PASS |
| III. UX Consistency via ABP Localization | Error messages (age validation) in localization JSON, use `BusinessException` | ✅ PASS |
| IV. Async-First Performance | All service methods async, use Specification pattern for search | ✅ PASS |
| V. No Manual Boilerplate | Use `IMultiTenant` for tenancy (not manual WHERE), `FullAuditedAggregateRoot` for audit | ✅ PASS |

**Gate Status**: ✅ PASSED - No violations

## Project Structure

### Documentation (this feature)

```text
specs/001-student-management/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (OpenAPI spec)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
src/
├── ClinicManagementSystem.Domain.Shared/
│   └── Localization/ClinicManagementSystem/
│       └── en.json                          # Student localization keys
├── ClinicManagementSystem.Domain/
│   └── Students/
│       ├── Student.cs                       # Entity (FullAuditedAggregateRoot)
│       ├── StudentManager.cs                # Domain service (age validation)
│       ├── IStudentRepository.cs            # Repository interface
│       └── StudentConsts.cs                 # Constants (max lengths)
├── ClinicManagementSystem.Application.Contracts/
│   └── Students/
│       ├── StudentDto.cs                    # Output DTO
│       ├── CreateStudentDto.cs              # Create input DTO
│       ├── UpdateStudentDto.cs              # Update input DTO
│       ├── GetStudentListDto.cs             # Paged request DTO
│       └── IStudentAppService.cs            # Service interface
├── ClinicManagementSystem.Application/
│   └── Students/
│       ├── StudentAppService.cs             # CrudAppService implementation
│       └── StudentAutoMapperProfile.cs      # AutoMapper config
├── ClinicManagementSystem.EntityFrameworkCore/
│   └── Students/
│       ├── StudentRepository.cs             # EfCoreRepository implementation
│       └── StudentEfCoreConfiguration.cs    # Fluent API config
└── ClinicManagementSystem.HttpApi/
    └── Students/
        └── StudentController.cs             # AbpController (auto-generated or manual)

test/
├── ClinicManagementSystem.Domain.Tests/
│   └── Students/
│       └── StudentManagerTests.cs           # Unit tests for age validation
└── ClinicManagementSystem.Application.Tests/
    └── Students/
        └── StudentAppServiceTests.cs        # Integration tests for CRUD
```

**Structure Decision**: ABP Layered DDD structure. Student feature follows standard ABP module organization with files grouped by feature (`Students/`) in each layer.

## Complexity Tracking

> No constitution violations - table not needed.
