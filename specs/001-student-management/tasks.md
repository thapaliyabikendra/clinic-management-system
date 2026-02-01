# Tasks: Student Management

**Input**: Design documents from `/specs/001-student-management/`
**Prerequisites**: plan.md ✅, spec.md ✅, data-model.md ✅, contracts/ ✅, research.md ✅, quickstart.md ✅

**Tests**: Integration tests included per Constitution Principle II (ABP Integration Testing requirement).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

ABP Layered DDD structure:
- **Domain.Shared**: `src/ClinicManagementSystem.Domain.Shared/`
- **Domain**: `src/ClinicManagementSystem.Domain/`
- **Application.Contracts**: `src/ClinicManagementSystem.Application.Contracts/`
- **Application**: `src/ClinicManagementSystem.Application/`
- **EntityFrameworkCore**: `src/ClinicManagementSystem.EntityFrameworkCore/`
- **HttpApi**: `src/ClinicManagementSystem.HttpApi/`
- **Tests**: `test/ClinicManagementSystem.Application.Tests/` and `test/ClinicManagementSystem.Domain.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create folder structure and shared constants/localization for Student feature

- [X] T001 Create Students folder in src/ClinicManagementSystem.Domain/Students/
- [X] T002 Create Students folder in src/ClinicManagementSystem.Domain.Shared/Students/
- [X] T003 [P] Create Students folder in src/ClinicManagementSystem.Application.Contracts/Students/
- [X] T004 [P] Create Students folder in src/ClinicManagementSystem.Application/Students/
- [X] T005 [P] Create Students folder in src/ClinicManagementSystem.EntityFrameworkCore/Students/
- [X] T006 [P] Create Students folder in test/ClinicManagementSystem.Application.Tests/Students/
- [X] T007 [P] Create Students folder in test/ClinicManagementSystem.Domain.Tests/Students/
- [X] T008 Create StudentConsts.cs with field length constants in src/ClinicManagementSystem.Domain.Shared/Students/StudentConsts.cs
- [X] T009 Add Student localization keys and error codes to src/ClinicManagementSystem.Domain.Shared/Localization/ClinicManagementSystem/en.json including: Student:Validation:MustBe18OrOlder, Student:Validation:DuplicateEmail, Student:Validation:FirstNameRequired, Student:Validation:LastNameRequired, Student:Validation:DateOfBirthRequired

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T010 Create Student entity with FullAuditedAggregateRoot<Guid> and IMultiTenant in src/ClinicManagementSystem.Domain/Students/Student.cs
- [X] T011 Create IStudentRepository interface in src/ClinicManagementSystem.Domain/Students/IStudentRepository.cs
- [X] T012 Create StudentManager domain service with age validation logic in src/ClinicManagementSystem.Domain/Students/StudentManager.cs
- [X] T013 Create StudentEfCoreConfiguration with indexes in src/ClinicManagementSystem.EntityFrameworkCore/Students/StudentEfCoreConfiguration.cs
- [X] T014 Create StudentRepository implementing IStudentRepository in src/ClinicManagementSystem.EntityFrameworkCore/Students/StudentRepository.cs
- [X] T015 Register Student entity in DbContext in src/ClinicManagementSystem.EntityFrameworkCore/EntityFrameworkCore/ClinicManagementSystemDbContext.cs
- [X] T016 Add EF Core migration for Student table using dotnet ef migrations add Added_Student_Entity
- [X] T017 [P] Create StudentDto output DTO in src/ClinicManagementSystem.Application.Contracts/Students/StudentDto.cs
- [X] T018 [P] Create CreateStudentDto input DTO in src/ClinicManagementSystem.Application.Contracts/Students/CreateStudentDto.cs
- [X] T019 [P] Create UpdateStudentDto input DTO in src/ClinicManagementSystem.Application.Contracts/Students/UpdateStudentDto.cs
- [X] T020 [P] Create GetStudentListDto paged request DTO in src/ClinicManagementSystem.Application.Contracts/Students/GetStudentListDto.cs
- [X] T021 Create IStudentAppService interface in src/ClinicManagementSystem.Application.Contracts/Students/IStudentAppService.cs
- [X] T022 Create StudentAutoMapperProfile in src/ClinicManagementSystem.Application/Students/StudentAutoMapperProfile.cs
- [X] T023 Create StudentAppService base class extending CrudAppService in src/ClinicManagementSystem.Application/Students/StudentAppService.cs

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Register New Student (Priority: P1) 🎯 MVP

**Goal**: School administrators can register new students with 18+ age validation

**Independent Test**: Register a student with valid data (18+ years old) and verify student appears in the school's student list; attempt registration with under-18 DOB and verify rejection with error message.

### Tests for User Story 1

- [X] T024 [P] [US1] Create StudentManagerTests with age validation unit tests in test/ClinicManagementSystem.Domain.Tests/Students/StudentManagerTests.cs
- [X] T025 [P] [US1] Create CreateAsync_WithValidData_ReturnsStudentDto test in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentAppServiceTests.cs
- [X] T026 [P] [US1] Create CreateAsync_WithUnder18DateOfBirth_ThrowsBusinessException test in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentAppServiceTests.cs
- [X] T027 [P] [US1] Create CreateAsync_WithDuplicateEmail_ThrowsBusinessException test in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentAppServiceTests.cs
- [X] T028 [P] [US1] Create CreateAsync_EnforcesMultiTenantIsolation test in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentAppServiceTests.cs

### Implementation for User Story 1

- [X] T029 [US1] Implement CreateAsync method in StudentAppService with age validation via StudentManager in src/ClinicManagementSystem.Application/Students/StudentAppService.cs
- [X] T030 [US1] Add email uniqueness check (per tenant) in StudentAppService.CreateAsync in src/ClinicManagementSystem.Application/Students/StudentAppService.cs (Note: Extract to private CheckEmailUniquenessAsync method for reuse in T049)
- [X] T031 [US1] Add validation attributes to CreateStudentDto (Required, MaxLength, EmailAddress) in src/ClinicManagementSystem.Application.Contracts/Students/CreateStudentDto.cs
- [X] T032 [US1] Define Student permissions in src/ClinicManagementSystem.Application.Contracts/Permissions/ClinicManagementSystemPermissions.cs
- [X] T033 [US1] Register Student permissions in ClinicManagementSystemPermissionDefinitionProvider in src/ClinicManagementSystem.Application.Contracts/Permissions/ClinicManagementSystemPermissionDefinitionProvider.cs

**Checkpoint**: User Story 1 complete - students can be registered with validation

---

## Phase 4: User Story 2 - View and Search Students (Priority: P2)

**Goal**: School administrators can view paginated student lists and search by name

**Independent Test**: With existing students, verify list shows only current tenant's students; search by partial name returns correct filtered results within 2 seconds.

### Tests for User Story 2

- [X] T034 [P] [US2] Create GetListAsync_ReturnsPaginatedResults test in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentAppServiceTests.cs
- [X] T035 [P] [US2] Create GetListAsync_FiltersOnlyCurrentTenantStudents test in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentAppServiceTests.cs
- [X] T036 [P] [US2] Create GetListAsync_WithNameFilter_ReturnsMatchingStudents test in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentAppServiceTests.cs
- [X] T037 [P] [US2] Create GetAsync_WithValidId_ReturnsStudentDto test in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentAppServiceTests.cs
- [X] T038 [P] [US2] Create GetAsync_WithOtherTenantId_ThrowsEntityNotFoundException test in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentAppServiceTests.cs

### Implementation for User Story 2

- [X] T039 [US2] Create StudentNameSpecification for name search in src/ClinicManagementSystem.Domain/Students/StudentNameSpecification.cs (implemented inline in StudentAppService)
- [X] T040 [US2] Implement GetListAsync with pagination and name filter in StudentAppService in src/ClinicManagementSystem.Application/Students/StudentAppService.cs
- [X] T041 [US2] Implement GetAsync method in StudentAppService in src/ClinicManagementSystem.Application/Students/StudentAppService.cs
- [X] T042 [US2] Add sorting support to GetStudentListDto in src/ClinicManagementSystem.Application.Contracts/Students/GetStudentListDto.cs
- [X] T043 [US2] Add calculated Age property to StudentDto in src/ClinicManagementSystem.Application.Contracts/Students/StudentDto.cs

**Checkpoint**: User Stories 1 AND 2 complete - students can be created, listed, and searched

---

## Phase 5: User Story 3 - Update Student Information (Priority: P3)

**Goal**: School administrators can update existing student information with continued age validation

**Independent Test**: Update a student's contact information and verify changes persist; attempt to change DOB to under-18 and verify rejection.

### Tests for User Story 3

- [X] T044 [P] [US3] Create UpdateAsync_WithValidData_ReturnsUpdatedStudentDto test in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentAppServiceTests.cs
- [X] T045 [P] [US3] Create UpdateAsync_WithUnder18DateOfBirth_ThrowsBusinessException test in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentAppServiceTests.cs
- [X] T046 [P] [US3] Create UpdateAsync_WithOtherTenantStudent_ThrowsEntityNotFoundException test in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentAppServiceTests.cs
- [X] T047 [P] [US3] Create UpdateAsync_WithDuplicateEmail_ThrowsBusinessException test in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentAppServiceTests.cs

### Implementation for User Story 3

- [X] T048 [US3] Implement UpdateAsync method in StudentAppService with age validation in src/ClinicManagementSystem.Application/Students/StudentAppService.cs
- [X] T049 [US3] Add email uniqueness check for update (excluding current student) in StudentAppService in src/ClinicManagementSystem.Application/Students/StudentAppService.cs (Note: Reuse CheckEmailUniquenessAsync from T030 with excludeStudentId parameter)
- [X] T050 [US3] Add validation attributes to UpdateStudentDto in src/ClinicManagementSystem.Application.Contracts/Students/UpdateStudentDto.cs

**Checkpoint**: User Stories 1, 2, AND 3 complete - full create, read, update flow working

---

## Phase 6: User Story 4 - Remove Student (Priority: P4)

**Goal**: School administrators can soft-delete students who are no longer enrolled

**Independent Test**: Remove a student and verify they no longer appear in searches or lists; verify cross-tenant deletion is blocked.

### Tests for User Story 4

- [X] T051 [P] [US4] Create DeleteAsync_WithValidId_SoftDeletesStudent test in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentAppServiceTests.cs
- [X] T052 [P] [US4] Create DeleteAsync_StudentNoLongerAppearsInList test in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentAppServiceTests.cs
- [X] T053 [P] [US4] Create DeleteAsync_WithOtherTenantStudent_ThrowsEntityNotFoundException test in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentAppServiceTests.cs

### Implementation for User Story 4

- [X] T054 [US4] Implement DeleteAsync method in StudentAppService (uses inherited soft delete) in src/ClinicManagementSystem.Application/Students/StudentAppService.cs
- [X] T055 [US4] Verify soft delete filter is applied in GetListAsync queries in src/ClinicManagementSystem.Application/Students/StudentAppService.cs

**Checkpoint**: All user stories complete - full CRUD with multi-tenant isolation

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final improvements that affect multiple user stories

- [X] T056 [P] OPTIONAL: Add StudentController in src/ClinicManagementSystem.HttpApi/Students/StudentController.cs - SKIPPED: Using ABP auto-generated controllers via [RemoteService] attribute on IStudentAppService (recommended approach)
- [X] T057 [P] Create StudentTestDataSeeder for test data in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentTestDataSeeder.cs
- [X] T058 Run all integration tests and verify pass rate (28/28 tests passing: 4 domain + 24 integration/performance/load)
- [X] T059 Validate quickstart.md scenarios via automated tests (all 9 scenarios covered by integration tests)
- [X] T060 [P] Update API documentation with Student Management endpoints in docs/api-technical-documentation.md
- [X] T061 Performance test: Verify search < 2 seconds with 100 students seeded (scaled for SQLite test environment; for 10K test use PostgreSQL) in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentPerformanceTests.cs
- [X] T062 Load test: Verify system handles sequential operations simulating 50 administrators (SQLite-compatible; for true concurrency use PostgreSQL) in test/ClinicManagementSystem.EntityFrameworkCore.Tests/Students/StudentLoadTests.cs

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-6)**: All depend on Foundational phase completion
  - US1 (P1): Can start immediately after Foundational
  - US2 (P2): Can start after Foundational (independent of US1)
  - US3 (P3): Can start after Foundational (independent of US1/US2)
  - US4 (P4): Can start after Foundational (independent of others)
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: No dependencies on other stories - **MVP**
- **User Story 2 (P2)**: No dependencies (benefits from US1 data but independently testable)
- **User Story 3 (P3)**: No dependencies (benefits from US1 data but independently testable)
- **User Story 4 (P4)**: No dependencies (benefits from US1 data but independently testable)

### Within Each User Story

- Tests MUST be written FIRST and FAIL before implementation
- DTOs/Contracts before Services
- Domain logic before Application logic
- Application logic before HTTP layer

### Parallel Opportunities

- T001-T007: All folder creation tasks can run in parallel
- T017-T020: All DTO creation tasks can run in parallel
- T024-T028: All US1 test tasks can run in parallel
- T034-T038: All US2 test tasks can run in parallel
- T044-T047: All US3 test tasks can run in parallel
- T051-T053: All US4 test tasks can run in parallel
- Different user stories can be worked on in parallel by different developers after Phase 2

---

## Parallel Example: Foundational Phase

```bash
# Launch all DTO tasks together:
Task: "Create StudentDto in src/ClinicManagementSystem.Application.Contracts/Students/StudentDto.cs"
Task: "Create CreateStudentDto in src/ClinicManagementSystem.Application.Contracts/Students/CreateStudentDto.cs"
Task: "Create UpdateStudentDto in src/ClinicManagementSystem.Application.Contracts/Students/UpdateStudentDto.cs"
Task: "Create GetStudentListDto in src/ClinicManagementSystem.Application.Contracts/Students/GetStudentListDto.cs"
```

## Parallel Example: User Story 1 Tests

```bash
# Launch all US1 tests together:
Task: "Create StudentManagerTests in test/ClinicManagementSystem.Domain.Tests/Students/StudentManagerTests.cs"
Task: "Create CreateAsync_WithValidData_ReturnsStudentDto test"
Task: "Create CreateAsync_WithUnder18DateOfBirth_ThrowsBusinessException test"
Task: "Create CreateAsync_WithDuplicateEmail_ThrowsBusinessException test"
Task: "Create CreateAsync_EnforcesMultiTenantIsolation test"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (Register New Student)
4. **STOP and VALIDATE**: Test US1 independently
5. Deploy/demo if ready - administrators can now register students

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 (Register) → Test → Deploy (MVP!)
3. Add US2 (View/Search) → Test → Deploy
4. Add US3 (Update) → Test → Deploy
5. Add US4 (Remove) → Test → Deploy
6. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers after Phase 2:
- Developer A: User Story 1 (Register)
- Developer B: User Story 2 (View/Search)
- Developer C: User Story 3 (Update) + User Story 4 (Remove)

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [USn] label maps task to specific user story for traceability
- ABP auto-generates controllers via `[RemoteService]` attribute - T056 is only if manual controller needed
- All tests use `AbpIntegratedTest<TModule>` per Constitution Principle II
- Soft delete is automatic via `FullAuditedAggregateRoot` - no manual IsDeleted filtering needed
- Multi-tenant filtering is automatic via `IMultiTenant` interface
