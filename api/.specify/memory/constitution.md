<!--
================================================================================
SYNC IMPACT REPORT
================================================================================
Version change: 2.0.0 → 3.0.0 (MAJOR - Added Mapperly and Speckit principles)

Modified principles:
- "II. ABP Integration Testing" → Updated test project structure and base classes
- Application Layer → Corrected from AutoMapper to Mapperly

Added sections:
- Principle VI: Object Mapping via Mapperly
- Principle VII: Spec-Driven Development (Speckit Workflow)

Clarifications:
- Test infrastructure: EntityFrameworkCore.Tests is PRIMARY location for integration tests
- Collection attribute REQUIRED for SQLite shared context
- Domain.Tests for pure unit tests (no DI preferred)

Templates requiring updates:
- .specify/templates/plan-template.md: ✅ Compatible (Constitution Check section exists)
- .specify/templates/spec-template.md: ✅ Compatible (Requirements section supports layer validation)
- .specify/templates/tasks-template.md: ✅ Compatible (Phase structure supports testing requirements)

Follow-up TODOs: None
================================================================================
-->

# Clinic Management System API Constitution

## Core Principles

### I. Strict DDD Layer Adherence

All code MUST respect ABP Framework's Domain-Driven Design layer boundaries:

- **Domain Layer** (`*.Domain`, `*.Domain.Shared`):
  - MUST contain: Entities, Aggregate Roots, Domain Services, Domain Events, Value Objects, Specifications, Repository Interfaces
  - MUST NOT contain: Application logic, DTOs, EF Core DbContext, API Controllers
  - Entities MUST inherit from appropriate ABP base classes (`Entity<TKey>`, `AggregateRoot<TKey>`, `FullAuditedAggregateRoot<TKey>`)
  - Domain Services MUST be stateless and focus on domain logic spanning multiple aggregates

- **Application Layer** (`*.Application`, `*.Application.Contracts`):
  - MUST contain: Application Services, DTOs, Mapperly Mappers, Application-level validation
  - MUST NOT contain: Domain logic, direct DbContext access, HTTP concerns
  - Application Services MUST inherit from `ApplicationService` or `CrudAppService<>`
  - DTOs MUST be defined in `*.Application.Contracts` for client consumption
  - Object mapping MUST use Mapperly (source generator) with extension method pattern (`entity.ToDto()`)

- **Infrastructure/EF Core Layer** (`*.EntityFrameworkCore`):
  - MUST contain: DbContext, Repository Implementations, EF Core Configurations, Migrations
  - MUST NOT contain: Business logic, Application Services, Controllers
  - Repository implementations MUST use ABP's `EfCoreRepository<>` base class
  - Database configurations MUST use Fluent API in `IEntityTypeConfiguration<>` classes

- **Web/API Layer** (`*.HttpApi`, `*.HttpApi.Host`):
  - MUST contain: Controllers, API-specific DTOs (if different from Application DTOs), Swagger configuration
  - MUST NOT contain: Business logic, direct repository access
  - Controllers MUST inherit from `AbpController` and delegate to Application Services
  - Controllers MUST NOT contain logic beyond HTTP concern handling

**Rationale**: Strict layer separation enables independent testing, clear ownership, and prevents the "big ball of mud" anti-pattern. ABP's architecture enforces these boundaries by design.

### II. ABP Integration Testing

All features MUST have integration tests using ABP's testing infrastructure:

- **Test Base Classes**: All integration tests MUST inherit from `AbpIntegratedTest<TModule>` or application-specific test base classes derived from it
- **Test Coverage Requirements**:
  - Application Services: 100% of public methods MUST have integration tests
  - Domain Services: MUST have unit tests; integration tests RECOMMENDED for complex cross-aggregate logic
  - API Endpoints: MUST have integration tests verifying HTTP status codes, response structure, and authorization
- **Test Project Structure**:
  - `*.Domain.Tests`: Pure unit tests for domain logic (no DI, no database, static methods preferred)
  - `*.EntityFrameworkCore.Tests`: Integration tests for application services using SQLite in-memory database (PRIMARY test location)
  - `*.Application.Tests`: Reserved for application-only tests without EF Core dependencies
  - `*.HttpApi.Client.Tests`: End-to-end API tests (optional, for client SDK validation)
- **Test Base Classes**:
  - Integration tests MUST use `ClinicManagementSystemEntityFrameworkCoreTestBase` (NOT `ApplicationTestBase`)
  - Integration tests MUST include `[Collection(ClinicManagementSystemTestConsts.CollectionDefinitionName)]` attribute for SQLite shared context
  - Domain unit tests SHOULD be pure (no DI) when possible for faster execution
- **Test Data Management**:
  - Tests MUST use ABP's `IDataSeeder` for consistent test data setup
  - Tests MUST NOT depend on execution order; use `[Collection]` attribute for shared context only when necessary
  - Each test method MUST leave the database in a clean state (use transactions or `WithUnitOfWork`)
- **Test Naming**: Tests MUST follow pattern `MethodName_Scenario_ExpectedResult` (e.g., `GetAsync_WithValidId_ReturnsPatient`)
- **Async Testing**: All async methods MUST be tested with `await` and MUST NOT use `.Result` or `.Wait()`

**Rationale**: ABP's integrated test infrastructure provides realistic testing with proper dependency injection, module configuration, and database context—catching integration issues that unit tests miss.

### III. UX Consistency via ABP Localization

All user-facing content MUST use ABP's localization and UI infrastructure:

- **String Localization**:
  - All user-facing strings MUST be defined in localization JSON files under `*.Domain.Shared/Localization/`
  - Hardcoded user-facing strings are PROHIBITED
  - Use `IStringLocalizer<TResource>` or `L["Key"]` syntax in Application Services and Controllers
  - Localization keys MUST follow naming convention: `{Feature}:{Category}:{Identifier}` (e.g., `Patient:Validation:NameRequired`)

- **Validation Messages**:
  - FluentValidation or Data Annotations MUST reference localized strings
  - Validation errors MUST use ABP's validation infrastructure (`IValidationErrorHandler`)
  - All validation MUST return localized messages via `AbpValidationException`

- **Error Handling**:
  - Business exceptions MUST inherit from `BusinessException` with localized `Message` and `Code`
  - Exception codes MUST follow pattern `ClinicManagement:{Feature}:{ErrorCode}` (e.g., `ClinicManagement:Appointment:TimeSlotNotAvailable`)
  - User-facing errors MUST NOT expose stack traces or internal details

- **API Response Consistency**:
  - All API responses MUST use ABP's standard response wrapper (automatic via `AbpController`)
  - Error responses MUST follow ABP's `RemoteServiceErrorResponse` structure
  - Pagination MUST use `PagedResultDto<T>` and `IPagedResultRequest`

- **Tag Helpers (for Razor views if applicable)**:
  - Forms MUST use ABP Tag Helpers (`abp-input`, `abp-select`, `abp-button`) for consistent styling
  - Validation summary MUST use `abp-validation-summary`

**Rationale**: Consistent localization and error handling improves user experience, enables internationalization, and reduces frontend/backend friction in a multi-language healthcare environment.

### IV. Async-First Performance

All I/O-bound operations MUST be asynchronous with optimized data access patterns:

- **Async Everywhere**:
  - All repository methods MUST be async (`GetAsync`, `InsertAsync`, etc.)
  - All Application Service methods MUST be async and return `Task<T>`
  - `async void` is PROHIBITED except for event handlers
  - Synchronous wrappers (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`) are PROHIBITED

- **Specification Pattern Optimization**:
  - Complex queries MUST use ABP's `Specification<T>` pattern for reusability
  - Specifications MUST include only necessary `Include()` statements to prevent over-fetching
  - Use `IQueryable` projections with `Select()` for read-only scenarios instead of loading full entities
  - Specifications MUST be defined in the Domain layer and used by repositories

- **Query Optimization**:
  - N+1 query patterns are PROHIBITED; use eager loading (`WithDetails()`) or explicit `Include()`
  - Bulk operations MUST use `InsertManyAsync`, `UpdateManyAsync`, `DeleteManyAsync`
  - Large result sets MUST use pagination (`IPagedResultRequest`)
  - Queries returning > 1000 records MUST be reviewed for optimization

- **Caching**:
  - Reference data (lookup tables, configuration) MUST use ABP's `IDistributedCache<T>`
  - Cache keys MUST follow pattern `{TenantId}:{EntityType}:{Identifier}`
  - Cache invalidation MUST be explicit on entity changes

- **Performance Targets**:
  - Simple CRUD operations: p95 < 200ms
  - Complex queries with joins: p95 < 500ms
  - Report generation: p95 < 2000ms (async with progress if longer)

**Rationale**: Healthcare systems handle concurrent users accessing patient records. Async operations prevent thread pool starvation; optimized queries prevent database bottlenecks.

### V. No Manual Boilerplate (ABP Native Features)

Features provided natively by ABP Framework MUST NOT be manually reimplemented:

- **Identity & Authentication** (PROHIBITED to reimplement):
  - User management MUST use `Volo.Abp.Identity` module
  - Role management MUST use ABP's `IdentityRole`
  - Authentication MUST use ABP's OpenIddict integration
  - Custom user properties MUST extend `IdentityUser` via ABP's extension system, NOT by creating parallel user entities

- **Audit Logging** (PROHIBITED to reimplement):
  - Entity change tracking MUST use ABP's `IAuditedObject` interfaces (`ICreationAuditedObject`, `IModificationAuditedObject`, `IDeletionAuditedObject`)
  - Use `FullAuditedAggregateRoot<T>` or `FullAuditedEntity<T>` for complete audit trails
  - Custom audit logging MUST extend ABP's `IAuditLogContributor`, NOT create parallel logging systems
  - Audit log storage configuration MUST use ABP's `AbpAuditLoggingOptions`

- **Multi-Tenancy** (PROHIBITED to reimplement):
  - Tenant isolation MUST use ABP's `IMultiTenant` interface
  - Tenant resolution MUST use ABP's built-in resolvers
  - Tenant-specific data MUST NOT use manual `WHERE TenantId = @id` clauses

- **Authorization** (PROHIBITED to reimplement):
  - Permissions MUST be defined via `PermissionDefinitionProvider`
  - Authorization checks MUST use `[Authorize(Policy)]` or `AuthorizationService.CheckAsync()`
  - Custom authorization logic MUST use ABP's `IPermissionChecker` or policy-based authorization

- **Feature Management** (PROHIBITED to reimplement):
  - Feature flags MUST use ABP's `IFeatureChecker`
  - Edition/tenant features MUST use `FeatureDefinitionProvider`

- **Exception Handling** (PROHIBITED to reimplement):
  - Global exception handling MUST rely on ABP's `IExceptionToErrorInfoConverter`
  - Custom error codes MUST use `BusinessException` with proper code assignment

- **Background Jobs** (PROHIBITED to reimplement):
  - Background processing MUST use ABP's `IBackgroundJobManager` or Hangfire integration
  - Recurring jobs MUST use ABP's `IBackgroundWorker` abstraction

**Rationale**: ABP provides battle-tested, well-integrated implementations of common features. Manual reimplementation introduces bugs, security vulnerabilities, and maintenance burden while losing framework integration benefits (e.g., tenant isolation, audit logging).

### VI. Object Mapping via Mapperly

All object mapping MUST use Mapperly (compile-time source generator), NOT AutoMapper:

- **Mapper Location**: `src/*.Application/{Feature}/{Feature}Mappers.cs`
- **Mapper Pattern**: Static partial class with extension methods
- **Mapper Attributes**: Use `[MapperIgnoreSource]` for ABP audit properties not needed in DTOs

**Required Ignored Properties**:
- `TenantId` - Internal multi-tenancy field
- `IsDeleted`, `DeleterId`, `DeletionTime` - Soft delete audit fields
- `ExtraProperties`, `ConcurrencyStamp` - ABP system fields

**Example**:
```csharp
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
```

**Usage**: `return student.ToDto();` (extension method pattern)

**Rationale**: Mapperly generates mapping code at compile-time, eliminating runtime reflection overhead and providing compile-time safety. Extension methods provide clean, fluent API.

### VII. Spec-Driven Development (Speckit Workflow)

All significant features MUST follow the spec-driven development workflow:

- **Specification Directory**: Features are specified in `specs/{feature-id}/` (relative to repo root)
- **Required Artifacts**:
  - `spec.md`: User stories with priorities (P1, P2, P3...), acceptance criteria, constraints
  - `plan.md`: Technical design, architecture decisions, technology choices
  - `tasks.md`: Implementation tasks organized by user story (generated)
- **Optional Artifacts**:
  - `data-model.md`: Entity definitions, relationships, constraints
  - `contracts/`: API endpoint specifications
  - `research.md`: Technical decisions, alternatives considered
  - `quickstart.md`: Validation scenarios, manual test checklist

**Workflow Sequence**:
1. `/speckit.specify` - Create feature specification from natural language
2. `/speckit.clarify` - Ask up to 5 clarification questions
3. `/speckit.plan` - Generate technical implementation plan
4. `/speckit.tasks` - Generate implementation tasks
5. `/speckit.implement` - Execute implementation
6. `/speckit.analyze` - Cross-artifact consistency check

**Task Organization**: Tasks MUST be organized by user story to enable independent implementation and testing:
- Phase 1: Setup (folder structure, constants, localization)
- Phase 2: Foundational (entities, repositories, domain services - BLOCKS all stories)
- Phase 3+: User stories in priority order (P1, P2, P3...)
- Final Phase: Polish & cross-cutting concerns

**Rationale**: Spec-driven development ensures requirements are captured before implementation, enables parallel development by multiple developers, and provides traceability from requirements to code.

## Development Workflow

All development activities MUST follow this workflow:

- **Branch Strategy**: Features branch from `develop`, hotfixes from `main`. Branch names MUST follow pattern `feature/###-description` or `fix/###-description`.
- **Code Review**: All changes MUST be reviewed by at least one other developer before merging. Reviewers MUST verify constitution compliance, especially layer boundaries and ABP pattern usage.
- **Commit Messages**: Commits MUST follow conventional commit format: `type(scope): description` (e.g., `feat(patient): add appointment scheduling endpoint`).
- **Pull Request Requirements**:
  - PRs MUST include description of changes and testing performed
  - All integration tests MUST pass
  - New features MUST include integration tests
  - Layer violations detected by static analysis MUST block merge
- **Documentation Updates**: Changes to API contracts, permissions, or localization MUST include corresponding documentation updates.

## Governance

This constitution represents the binding standards for all development on the Clinic Management System.

- **Precedence**: This constitution supersedes all informal practices. When conflicts arise, constitution rules take priority. ABP Framework documentation is authoritative for framework-specific implementation details.
- **Compliance Verification**: All pull requests MUST be checked against relevant principles. Reviewers MUST verify:
  - Layer boundary compliance (Principle I)
  - Integration test coverage (Principle II)
  - Localization usage (Principle III)
  - Async patterns (Principle IV)
  - No prohibited boilerplate (Principle V)
- **Amendment Process**:
  1. Propose change via pull request to constitution file
  2. Require approval from at least two senior developers
  3. Document rationale and migration plan for existing code
  4. Update version number according to semantic versioning
- **Exception Handling**: Temporary exceptions MAY be granted with documented justification and remediation timeline. Exceptions MUST be tracked and reviewed quarterly. Exceptions to Principle V (No Manual Boilerplate) require architectural review.
- **Runtime Guidance**: For day-to-day development decisions not covered here, refer to the [ABP Framework documentation](https://abp.io/docs/latest) and Microsoft .NET coding guidelines.

**Version**: 3.0.0 | **Ratified**: 2026-01-27 | **Last Amended**: 2026-01-27
