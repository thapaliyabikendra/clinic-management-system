# Research: Student Management

**Feature**: 001-student-management
**Date**: 2026-01-27

## ABP Multi-Tenancy for Student Entity

**Decision**: Use `IMultiTenant` interface on Student entity

**Rationale**: ABP automatically filters queries by `TenantId` when entity implements `IMultiTenant`. This provides:
- Automatic tenant isolation without manual WHERE clauses
- Integration with ABP's `ICurrentTenant` service
- Consistent behavior across all repositories

**Alternatives Considered**:
- Manual `TenantId` filtering: Rejected - violates Constitution Principle V (No Manual Boilerplate)
- Separate databases per tenant: Rejected - overkill for this scale; shared database with row-level isolation is sufficient

## Age Validation Implementation

**Decision**: Implement age validation in `StudentManager` domain service

**Rationale**:
- Age validation is domain logic (business rule: 18+ years)
- Domain service allows reuse across create and update operations
- Throws `BusinessException` with localized message for ABP error handling

**Alternatives Considered**:
- DTO validation attribute: Rejected - validation depends on current server date, not suitable for declarative validation
- Application service validation: Rejected - business rule belongs in domain layer per DDD

**Implementation Pattern**:
```csharp
public class StudentManager : DomainService
{
    public void ValidateAge(DateTime dateOfBirth)
    {
        var age = CalculateAge(dateOfBirth, Clock.Now);
        if (age < 18)
        {
            throw new BusinessException(
                code: "ClinicManagement:Student:MustBe18OrOlder",
                message: L["Student:Validation:MustBe18OrOlder"]
            );
        }
    }
}
```

## Email Uniqueness Constraint

**Decision**: Database unique index on (TenantId, Email) with application-level check

**Rationale**:
- Unique per tenant (clarification from spec)
- Database index provides final safety net
- Application-level check provides user-friendly error before database exception

**Alternatives Considered**:
- Global uniqueness: Rejected - conflicts with multi-tenant isolation requirement
- Application-only check: Rejected - race conditions possible without database constraint

## Soft Delete Strategy

**Decision**: Use `FullAuditedAggregateRoot<Guid>` which includes `ISoftDelete`

**Rationale**:
- ABP automatically filters soft-deleted records from queries
- Provides audit trail (CreationTime, CreatorId, LastModificationTime, LastModifierId, DeletionTime, DeleterId)
- Satisfies spec requirement for audit retention

**Alternatives Considered**:
- Hard delete: Rejected - violates audit retention requirement
- Custom IsDeleted field: Rejected - ABP provides this via `ISoftDelete`

## Search Implementation

**Decision**: Use `Specification<Student>` pattern for name search

**Rationale**:
- Constitution Principle IV requires Specification pattern for complex queries
- Reusable across different search scenarios
- Composable with other specifications

**Implementation Pattern**:
```csharp
public class StudentNameSpecification : Specification<Student>
{
    private readonly string _filter;

    public StudentNameSpecification(string filter)
    {
        _filter = filter?.ToLowerInvariant();
    }

    public override Expression<Func<Student, bool>> ToExpression()
    {
        if (string.IsNullOrWhiteSpace(_filter))
            return _ => true;

        return s => s.FirstName.ToLower().Contains(_filter)
                 || s.LastName.ToLower().Contains(_filter);
    }
}
```

## Pagination

**Decision**: Use ABP's `PagedAndSortedResultRequestDto` and `PagedResultDto<T>`

**Rationale**:
- Constitution Principle III requires ABP pagination patterns
- Consistent with other ABP services
- Built-in support in `CrudAppService<>`

## Performance Considerations

**Decision**: Index strategy for common queries

**Indexes Required**:
1. `IX_Students_TenantId` - Tenant isolation queries (automatic via ABP)
2. `IX_Students_TenantId_Email` - Unique constraint + lookup
3. `IX_Students_TenantId_FirstName_LastName` - Search performance

**Rationale**: Supports p95 < 500ms target for 10,000 students per tenant
