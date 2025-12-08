using System.Net.Http.Json;
using NUnit.Framework.Legacy;
using Study.E2E.Tests.Clients;
using Study.E2E.Tests.Models;

namespace Study.App.E2E.Tests
{
    [TestFixture]
    [Category("E2E")]
    public class StudyGroupE2ETests
    {
        [SetUp]
        public async Task SetUp()
        {
            using var client = new StudyGroupClient();
            await client.CleanupAllStudyGroupsAsync();
        }
        
        [Test]
        [Property("TCID", "E2E-01")]
        public async Task CreateStudyGroup_And_PreventDuplicateForSameSubject()
        {
            using var client = new StudyGroupClient();

            var uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 6);
            var firstRequest = new CreateStudyGroupRequest
            {
                Name = $"Math Group {uniqueSuffix}",
                Subject = Subject.Math
            };

            var secondRequest = new CreateStudyGroupRequest
            {
                Name = $"Another Math Group {uniqueSuffix}",
                Subject = Subject.Math
            };

            // 1) Create the first Math study group
            var firstResponse = await client.CreateStudyGroupRawAsync(firstRequest);

            Assert.That((int)firstResponse.StatusCode, Is.EqualTo(201), "First group creation should succeed.");

            var createdGroup = await firstResponse.Content.ReadFromJsonAsync<StudyGroup>();
            Assert.That(createdGroup, Is.Not.Null);
            Assert.That(createdGroup!.Subject, Is.EqualTo(Subject.Math));

            // 2) Attempt to create a second Math group (duplicate subject)
            var secondResponse = await client.CreateStudyGroupRawAsync(secondRequest);

            Assert.That((int)secondResponse.StatusCode, Is.EqualTo(409), "Second Math group should be rejected.");
            var error = await secondResponse.Content.ReadAsStringAsync();
            StringAssert.Contains("already exists", error);

            // 3) Verify the first group still exists in the system (list API)
            var allGroups = await client.GetStudyGroupsAsync(subject: "Math");
            Assert.That(allGroups.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(allGroups.Any(g => g.StudyGroupId == createdGroup.StudyGroupId), Is.True);
        }
        
        [Test]
        [Property("TCID", "E2E-02")]
        public async Task JoinAndLeaveStudyGroups_FullFlow()
        {
            using var client = new StudyGroupClient();

            var rand = Guid.NewGuid().ToString("N").Substring(0, 5);
            const int userId = 1;

            // 1) Create Math and Chemistry groups
            var mathGroup = await client.CreateStudyGroupAsync(new CreateStudyGroupRequest
            {
                Name = $"Math Group {rand}",
                Subject = Subject.Math
            });
            var chemGroup = await client.CreateStudyGroupAsync(new CreateStudyGroupRequest
            {
                Name = $"Chem Group {rand}",
                Subject = Subject.Chemistry
            });

            Assert.That(mathGroup, Is.Not.Null);
            Assert.That(chemGroup, Is.Not.Null);

            // 2) Join Math and Chem groups as user #1
            var joinMath = await client.JoinStudyGroupRawAsync(mathGroup!.StudyGroupId, userId);
            joinMath.EnsureSuccessStatusCode();

            var joinChem = await client.JoinStudyGroupRawAsync(chemGroup!.StudyGroupId, userId);
            joinChem.EnsureSuccessStatusCode();

            // 3) Verify membership by reading groups (simulating "My groups" view)
            var mathFromApi = await client.GetStudyGroupByIdAsync(mathGroup.StudyGroupId);
            var chemFromApi = await client.GetStudyGroupByIdAsync(chemGroup.StudyGroupId);

            Assert.That(mathFromApi!.Users.Any(u => u.UserId == userId), Is.True,
                "User #1 should be member of Math group.");
            Assert.That(chemFromApi!.Users.Any(u => u.UserId == userId), Is.True,
                "User #1 should be member of Chem group.");

            // 4) Leave one group (Chemistry)
            var leaveChem = await client.LeaveStudyGroupRawAsync(chemGroup.StudyGroupId, userId);
            leaveChem.EnsureSuccessStatusCode();

            // 5) Verify membership updated
            chemFromApi = await client.GetStudyGroupByIdAsync(chemGroup.StudyGroupId);
            Assert.That(chemFromApi!.Users.Any(u => u.UserId == userId), Is.False,
                "User #1 should no longer be member of Chem group after leaving.");
            
            mathFromApi = await client.GetStudyGroupByIdAsync(mathGroup.StudyGroupId);
            Assert.That(mathFromApi!.Users.Any(u => u.UserId == userId), Is.True,
                "User #1 should still be member of Math group.");
        }
        
        [Test]
        [Property("TCID", "E2E-03")]
        public async Task FilterAndSortStudyGroups_FullFlow()
        {
            using var client = new StudyGroupClient();

            var rand = Guid.NewGuid().ToString("N").Substring(0, 5);

            // 1) Create multiple groups with different subjects
            var g1 = await client.CreateStudyGroupAsync(new CreateStudyGroupRequest
            {
                Name = $"Oldest Math {rand}",
                Subject = Subject.Math
            });

            await Task.Delay(20);

            var g2 = await client.CreateStudyGroupAsync(new CreateStudyGroupRequest
            {
                Name = $"Middle Chem {rand}",
                Subject = Subject.Chemistry
            });

            await Task.Delay(20);

            var g3 = await client.CreateStudyGroupAsync(new CreateStudyGroupRequest
            {
                Name = $"Newest Physics {rand}",
                Subject = Subject.Physics
            });

            // 2) Browse all groups (simulating main list view)
            var allGroups = await client.GetStudyGroupsAsync();
            Assert.That(allGroups.Count, Is.GreaterThanOrEqualTo(3),
                "We expect at least the 3 groups we just created.");

            // 3) Filter by subject (Math)
            var mathGroups = await client.GetStudyGroupsAsync(subject: "Math");
            Assert.That(mathGroups.Any(g => g.Subject == Subject.Math), Is.True);
            Assert.That(mathGroups.All(g => g.Subject == Subject.Math), Is.True);

            // 4) Sort by newest first
            var descSorted = await client.GetStudyGroupsAsync(sortOrder: "desc");
            var descNames = descSorted.Select(g => g.Name).ToList();

            Assert.That(descNames.Contains(g3!.Name), Is.True, "Newest Physics group should appear in desc-sorted list.");

            // 5) Sort by oldest first
            var ascSorted = await client.GetStudyGroupsAsync(sortOrder: "asc");
            var ascNames = ascSorted.Select(g => g.Name).ToList();

            Assert.That(ascNames.Contains(g1!.Name), Is.True, "Oldest Math group should appear in asc-sorted list.");
        }
    }
}