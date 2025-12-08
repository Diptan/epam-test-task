# Approach and Reasoning

## Table of Contents

- [Approach and Reasoning](#approach-and-reasoning)
- [Assumptions About the Application](#assumptions-about-the-application)
- [Acceptance Criteria Coverage](#acceptance-criteria-coverage)
- [Test Cases](#test-cases)
- [SQL Query Task](#sql-query-task)
- [How to Run Tests](#how-to-run-tests)

---

**Approach:** To ensure the new Study Groups feature meets all requirements, I adopt a layered testing strategy spanning Unit, Integration, and End-to-End (E2E) tests. While this approach conceptually aligns with the testing pyramid principles - promoting fast, reliable lower-level testing (shift-left approach) - in practice it takes a more integration-focused shape rather than a strict pyramid.

While unit tests are useful for isolating small pieces of logic, my preference is to emphasize integration tests, as they more accurately reflect real-world scenarios by validating the interaction between controllers, services, repositories, and the database, rather than testing isolated methods. This provides higher confidence in overall system behavior, even if it results in a flatter or “hourglass-shaped” distribution of tests instead of a traditional pyramid.

1. **Unit Tests** - test individual components in isolation (model validation, business logic, exceptions and errors handling tests).
   - Run in the build pipeline (part of CI process).
   - Use mocks for internal code dependencies.
  
     https://github.com/Diptan/epam-test-task/blob/main/study-app/Study.Unit.Tests/StudyGroupDomainTests.cs

2. **Component/Integration Tests** (referred to as Integration Tests hereafter) - confirm that the controller, service, data layer, and database work together as expected with mocked or in-memory database or real db created as docker container (e.g. creation persists to DB, filters and sorting return correct results, join/leave operations update relations).
   - Executing real HTTP requests against the service.
   - Run in build pipeline against locally started service (prior to deployment; part of CI process).
   - Almost everything that can be verified at the unit level can also be tested within integration scope, often with still acceptable execution time.
  
     https://github.com/Diptan/epam-test-task/blob/main/study-app/Study.Integration.Tests/Tests/StudyGroupControllerTests.cs
     
   - However, if a large number of integration tests significantly increase runtime - e.g., execution time exceeds 15 minutes and becomes an issue for CI/CD (especially when we have frequent deployments: each hour etc.) - it makes sense to shift some of them down to unit level to improve performance (example: unit test execution time 0.1 sec and integration test 1 sec, so if we have 1000 tests, results are 100 seconds vs ~17 minutes).

     Here is an example, how we can rewrite test from integration to unit level to check controller: 
     https://github.com/Diptan/epam-test-task/blob/main/study-app/Study.Unit.Tests/StudyGroupControllerUnitTests.cs

3. **E2E API Tests** - simulate high-level user scenarios through the API client, complementing manual UI testing by covering similar end-to-end flows without a GUI. They validate that all backend services work together to support key business journeys.
   - Executed post-deployment in real environments DEV/TEST/PROD (part of CD process).
   - These tests ensure the deployed system functions correctly as a whole - free from configuration, network, or connectivity issues. They verify that databases, integrations, and external services are properly connected and accessible.
  
     https://github.com/Diptan/epam-test-task/blob/main/study-app-e2e-tests/Study.E2E.Tests/Tests/StudyGroupE2ETests.cs

4. **E2E Manual Tests (UI)** - validate that the system supports key user journeys end-to-end through the actual web interface (e.g. creating a Study Group, joining/leaving groups, browsing/filtering/sorting groups).
   - Run post-deployment against the deployed environment (TEST/PROD), focusing on business-critical, mostly happy-path flows that represent how students really use the app. These scenarios are included in the regression pack but kept minimal.
   - Manual E2E tests therefore act as a final safety net to confirm that the front-end, back-end, routing, configuration and real user interactions all work together as expected, while the bulk of functional coverage remains at the API (E2E) and integration layers

---

Yes, I have implemented a similar layered testing approach in a previous project based on microservices architecture.

In that project:
1. The manual QA team first provided the full scope of manual test cases covering all functional and user journeys.
2. The QA and Automation QA (AQA) teams jointly reviewed these cases, defined priorities, and determined the automation scope to ensure coverage for critical business flows.
3. The AQA team then categorized tests by testing level, deciding whether each scenario should be automated as an integration (backend team) test, an E2E API test (aqa team), component (frontend team) or an E2E UI (aqa team) test depending on its business impact and technical complexity.
---

## Assumptions About the Application
https://github.com/Diptan/epam-test-task/tree/main/study-app/Study.API

1. The web application has a UI where users can navigate to a "Study Groups" section
2. Users must be logged in to create, join, or leave study groups
3. The UI displays validation error messages when invalid inputs are provided
4. There's a list view showing all study groups with filtering and sorting options

---

## Acceptance Criteria Coverage

| AC | Description | Test Types | Count |
|----|-------------|------------|-------|
| 1 | One study group per subject | Unit, Integration, E2E | 4 |
| 1a | Name 5–30 characters | Unit, Integration, E2E | 5 |
| 1b | Only valid subjects: Math, Chemistry, Physics | Unit, Integration, E2E | 3 |
| 1c | Record when Study Groups were created | Integration, E2E | 3 |
| 2 | Users can join Study Groups for different subjects | Unit, Integration, E2E | 6 |
| 3 | Users can check the list of all existing Study Groups | Integration, E2E | 6 |
| 3a | Users can filter Study Groups by a given subject | Integration, E2E | 3 |
| 3b | Users can sort by most recent / oldest study groups | Integration, E2E | 3 |
| 4 | Users can leave Study Groups they joined | Unit, Integration, E2E | 5 |

---

## Test Cases

| TC ID | Acceptance Criteria | Title | Testing Level (comma separated) | Main Scope | High-level Steps (summary) | Priority | In Regression? | Automation status |
|-------|---------------------|-------|--------------------------------|------------|---------------------------|----------|----------------|-------------------|
| TC-01 | 1, 1a, 1b, 1c | Create valid Study Group for a valid subject | Unit, Integration | Domain validation + API/DB creation | Unit: Call StudyGroup constructor with valid names (length 5–29) and valid subject. Integration: POST /api/studygroup with valid body, then read created group. | High | ✅ Yes | ✅ Automated |
| TC-02 | 1 | Prevent multiple Study Groups for same subject | Integration | API + repository uniqueness rule | Use API: create Math group, then try to create second Math group via POST /api/studygroup. | High | ✅ Yes | ✅ Automated |
| TC-03 | 1a | Reject Study Group name shorter than 5 characters | Unit | Domain length validation | Call StudyGroup constructor with name length 4. | High | ✅ Yes | ✅ Automated |
| TC-04 | 1a | Reject Study Group name longer than 30 characters | Unit | Domain length validation | Call StudyGroup constructor with name > 30 chars. | High | ✅ Yes | ✅ Automated |
| TC-05 | 1b | Reject Study Group with invalid subject | Unit (controller-level) | Controller / request validation for Subject enum | Call controller action (or API) with invalid enum value (Subject)999. | High | ✅ Yes | ✅ Automated |
| TC-06 | 1c | Verify createdAt timestamp stored and correct format | Integration | Repository persistence | Create StudyGroup via repo/API, then read from DB and inspect CreateDate. | High | ✅ Yes | ✅ Automated |
| TC-07 | 2 | User can join Study Groups for different subjects | Integration | Join endpoint + many-to-many mapping | Seed user + create groups (Math, Chem, Physics) via API; call POST /join for each; reload user with Include(StudyGroups). | High | ✅ Yes | ✅ Automated |
| TC-08 | 2 | Prevent duplicate join to same Study Group | Unit, Integration | Domain AddUser logic + Join endpoint behavior | Unit: Call AddUser twice with same user, ensure only one entry in Users. Integration: Use API to join same group twice; second call returns 400. | High | ✅ Yes | ✅ Automated |
| TC-09 | 2 | Joining a non-existing Study Group | Integration | Join endpoint error handling | Seed user; call POST /join/{nonExistingId}/{userId}. | High | ✅ Yes | ✅ Automated |
| TC-10 | 3 | List all existing Study Groups | Integration | GET list endpoint | Create 3 groups via API; call GET /api/studygroup. | High | ✅ Yes | ✅ Automated |
| TC-11 | 3a | Filter Study Groups by subject with matches | Integration | Filtering via subject query param | Create Math + Chem groups; call GET /api/studygroup?subject=Math. | High | ✅ Yes | ✅ Automated |
| TC-12 | 3a | Filter Study Groups by subject with no matches | Integration | Filtering edge case | Create Math + Chem groups; call GET /api/studygroup?subject=Physics. | High | ✅ Yes | ✅ Automated |
| TC-13 | 3b | Sort Study Groups by most recently created first | Integration | Sorting logic "desc" / "newest" | Create groups in known order (Oldest, Middle, Newest). Call GET /api/studygroup?sortOrder=desc. | High | ✅ Yes | ✅ Automated |
| TC-14 | 3b | Sort Study Groups by oldest first | Integration | Sorting logic "asc" / "oldest" | Create groups in known order; call GET /api/studygroup?sortOrder=asc. | High | ✅ Yes | ✅ Automated |
| TC-15 | 4 | User leaves a Study Group they joined | Integration | Leave endpoint + membership removal | Seed user + group; join via API; call POST /leave; reload group including users. | High | ✅ Yes | ✅ Automated |
| TC-16 | 4 | User tries to leave a Study Group they are not a member of | Unit, Integration | Domain RemoveUser semantics + Leave endpoint error | Unit: Call RemoveUser with non-member, list unchanged. Integration: Seed user + group; do not join; call POST /leave; check response. | High | ✅ Yes | ✅ Automated |
| E2E-01 | 1, 1a, 1b, 1c | Create a Study Group and prevent duplicates via UI | E2E API, E2E manual | Full browser/UI flow | In UI: log in → open "Create Study Group" → create Math group → attempt second Math group. | Critical | ✅ Yes | ✅ Automated |
| E2E-02 | 2, 4 | Join and leave Study Groups through UI | E2E API, E2E manual | Full browser/UI join/leave flow | Via UI: log in → join Math + Chem → verify in "My groups" → leave one → verify it disappears and counts update. | Critical | ✅ Yes | ✅ Automated |
| E2E-03 | 3, 3a, 3b | Browse, filter, and sort Study Groups via UI | E2E API, E2E manual | Full UI list/filter/sort | Open Study Groups page → check full list → filter by subject → clear → sort by newest/oldest and verify. | Critical | ✅ Yes | ✅ Automated |
| E2E-04 | 1a, 1b | Create Study Group with boundary name lengths | E2E manual | UI validation at boundaries | In UI: create Study Group with name length exactly 5, exactly 30, and then with 4 and 31 chars. Try for different subjects. | Medium | ❌ No | ❌ Not automated |
| E2E-05 | 1a, 1b | Create Study Group with leading/trailing spaces in name | E2E manual | Trimming & validation behavior | In UI: enter "   Math Group   " (spaces) and create; then try "    " only spaces. | Medium | ❌ No | ❌ Not automated |
| E2E-06 | 2 | Joining/leaving same group repeatedly and from multiple tabs | E2E manual | Idempotency & multi-tab behavior | Open Study Groups UI in two tabs as same user. Tab A: join Math group. Tab B: hit join Math again, then leave; then refresh both tabs. | Low | ❌ No | ❌ Not automated |

---

## SQL Query Task

**Task:** Write a SQL query that will return "all the StudyGroups which have at least one user with 'name' starting on 'M' sorted by 'creation date'" like "Miguel" or "Manuel"

**Solution:**

```sql
SELECT DISTINCT sg.*
FROM StudyGroups AS sg
JOIN StudyGroupUsers AS sgu 
    ON sg.StudyGroupId = sgu.StudyGroupId
JOIN Users AS u 
    ON sgu.UserId = u.UserId
WHERE u.Name LIKE 'M%'
ORDER BY sg.CreateDate;
```

---

## How to Run Tests

### Unit and Integration Tests

```bash
git clone https://github.com/Diptan/epam-test-task.git
cd study-app
dotnet test
```

### E2E API Tests

**Terminal 1** - Start the application:
```bash
cd study-app
dotnet run
```

**Terminal 2** - Run E2E tests:
```bash
cd study-app-e2e-tests
dotnet test
```

---
