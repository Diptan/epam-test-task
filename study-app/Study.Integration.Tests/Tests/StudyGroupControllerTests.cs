using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Legacy;
using study_app.DTOs;
using Study.API.Data.Models;
using Study.Integration.Tests.Infrastructure;

namespace Study.Integration.Tests.Tests;

public class StudyGroupControllerTests
{
    [Test]
    [Property("TCID", "TC-01")]
    public async Task CreateStudyGroup_ShouldReturnCreated_WhenValidRequest()
    {
        using var service = new StudyGroupApiService();
    
        var request = new CreateStudyGroupRequest
        {
            Name = "Algebra Wizards",
            Subject = Subject.Math
        };
    
        // Act
        var response = await service.CreateStudyGroupRawAsync(request);
    
        // Assert
        Assert.That((int)response.StatusCode, Is.EqualTo(201)); // Created
    
        var created = await response.Content.ReadFromJsonAsync<StudyGroup>();
        Assert.That(created!.StudyGroupId, Is.GreaterThan(0));
        Assert.That(created.Name, Is.EqualTo(request.Name));
        Assert.That(created.Subject, Is.EqualTo(request.Subject));
        Assert.That(created.CreateDate, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    [Property("TCID", "TC-02")]
    public async Task CreateStudyGroup_ShouldReturnConflict_WhenSubjectAlreadyHasGroup()
    {
        using var service = new StudyGroupApiService();

        var first = new CreateStudyGroupRequest
        {
            Name = "Math Group 1",
            Subject = Subject.Math
        };
        var second = new CreateStudyGroupRequest
        {
            Name = "Math Group 2",
            Subject = Subject.Math
        };

        // Arrange: first one succeeds
        var firstResponse = await service.CreateStudyGroupRawAsync(first);
        firstResponse.EnsureSuccessStatusCode();

        // Act: second one should conflict
        var secondResponse = await service.CreateStudyGroupRawAsync(second);

        // Assert
        Assert.That((int)secondResponse.StatusCode, Is.EqualTo(409)); // Conflict
        var error = await secondResponse.Content.ReadAsStringAsync();
        StringAssert.Contains("already exists", error);
    }

    [Test]
    [Property("TCID", "TC-10")]
    public async Task GetStudyGroups_ShouldReturnAllExistingGroups()
    {
        using var service = new StudyGroupApiService();

        // Arrange
        await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Math Group",
            Subject = Subject.Math
        });
        await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Chem Group",
            Subject = Subject.Chemistry
        });
        var gg = await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Physics Group",
            Subject = Subject.Physics
        });

        // Act
        var groups = await service.GetStudyGroupsAsync();

        // Assert
        Assert.That(groups.Count, Is.EqualTo(3));
        var subjects = groups.Select(g => g.Subject).ToArray();
        CollectionAssert.AreEquivalent(
            new[] { Subject.Math, Subject.Chemistry, Subject.Physics },
            subjects);
    }

    [Test]
    [Property("TCID", "TC-11")]
    public async Task GetStudyGroups_ShouldFilterBySubject()
    {
        using var service = new StudyGroupApiService();

        // Arrange
        await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Math Group",
            Subject = Subject.Math
        });
        await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Chem Group",
            Subject = Subject.Chemistry
        });

        // Act
        var mathGroups = await service.GetStudyGroupsAsync(subject: "Math");

        // Assert
        Assert.That(mathGroups, Is.Not.Empty);
        Assert.That(mathGroups.All(g => g.Subject == Subject.Math), Is.True);
    }

    [Test]
    [Property("TCID", "TC-13")]
    public async Task GetStudyGroups_ShouldSortByNewestFirst_WhenSortOrderDesc()
    {
        using var service = new StudyGroupApiService();

        // Arrange: create groups in known order
        var g1 = await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Oldest Group",
            Subject = Subject.Math
        });

        await Task.Delay(10); // ensure different timestamps in in-memory DB

        var g2 = await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Middle Group",
            Subject = Subject.Chemistry
        });

        await Task.Delay(10);

        var g3 = await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Newest Group",
            Subject = Subject.Physics
        });

        // Act
        var groups = await service.GetStudyGroupsAsync(sortOrder: "desc"); // newest first
        var names = groups.Select(g => g.Name).ToList();

        // Assert
        Assert.That(names[0], Is.EqualTo("Newest Group"));
        Assert.That(names[1], Is.EqualTo("Middle Group"));
        Assert.That(names[2], Is.EqualTo("Oldest Group"));
    }

    [Test]
    [Property("TCID", "TC-14")]
    public async Task GetStudyGroups_ShouldSortByOldestFirst_WhenSortOrderAsc()
    {
        using var service = new StudyGroupApiService();

        // Arrange: create groups in known order
        var g1 = await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Oldest Group",
            Subject = Subject.Math
        });

        await Task.Delay(10);

        var g2 = await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Middle Group",
            Subject = Subject.Chemistry
        });

        await Task.Delay(10);

        var g3 = await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Newest Group",
            Subject = Subject.Physics
        });

        // Act
        var groups = await service.GetStudyGroupsAsync(sortOrder: "asc"); // oldest first
        var names = groups.Select(g => g.Name).ToList();

        // Assert
        Assert.That(names[0], Is.EqualTo("Oldest Group"));
        Assert.That(names[1], Is.EqualTo("Middle Group"));
        Assert.That(names[2], Is.EqualTo("Newest Group"));
    }

    [Test]
    [Property("TCID", "TC-07")]
    public async Task JoinStudyGroup_ShouldAllowUserToJoinGroupsOfDifferentSubjects()
    {
        using var service = new StudyGroupApiService();

        // Arrange: seed user + groups directly via DbContext
        using var scope = service.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StudyGroupDbContext>();

        var user = new User(0, "kenobi", "kenobi@example.com");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var math = await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Math Group",
            Subject = Subject.Math
        });

        var chem = await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Chem Group",
            Subject = Subject.Chemistry
        });

        var physics = await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Physics Group",
            Subject = Subject.Physics
        });

        // Act
        var r1 = await service.JoinStudyGroupRawAsync(math!.StudyGroupId, user.UserId);
        var r2 = await service.JoinStudyGroupRawAsync(chem!.StudyGroupId, user.UserId);
        var r3 = await service.JoinStudyGroupRawAsync(physics!.StudyGroupId, user.UserId);

        r1.EnsureSuccessStatusCode();
        r2.EnsureSuccessStatusCode();
        r3.EnsureSuccessStatusCode();

        // Assert
        var userFromDb = await db.Users
            .Include(u => u.StudyGroups)
            .FirstAsync(u => u.UserId == user.UserId);

        var subjects = userFromDb.StudyGroups.Select(g => g.Subject).ToArray();
        CollectionAssert.AreEquivalent(
            new[] { Subject.Math, Subject.Chemistry, Subject.Physics },
            subjects);
    }
    
    [Test]
    [Property("TCID", "TC-09")]
    public async Task JoinStudyGroup_ShouldReturnNotFound_WhenGroupDoesNotExist()
    {
        using var service = new StudyGroupApiService();

        // Arrange: create a real user in the test DB
        using var scope = service.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StudyGroupDbContext>();

        var user = new User(0, "kenobi", "kenobi@example.com");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var nonExistingGroupId = 9999;

        // Act
        var response = await service.JoinStudyGroupRawAsync(nonExistingGroupId, user.UserId);

        // Assert
        Assert.That((int)response.StatusCode, Is.EqualTo(404));
        var error = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("Study group with ID", error);
        StringAssert.Contains("not found", error);
    }
    
    [Test]
    [Property("TCID", "TC-12")]
    public async Task GetStudyGroups_ShouldReturnEmptyList_WhenSubjectHasNoGroups()
    {
        using var service = new StudyGroupApiService();

        // Arrange
        await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Math Group",
            Subject = Subject.Math
        });
        await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Chem Group",
            Subject = Subject.Chemistry
        });

        // Act
        var physicsGroups = await service.GetStudyGroupsAsync(subject: "Physics");

        // Assert
        Assert.That(physicsGroups, Is.Not.Null);
        Assert.That(physicsGroups.Count, Is.EqualTo(0));
    }
    
    [Test]
    [Property("TCID", "TC-06")]
    public async Task CreateStudyGroup_ShouldSetCreateDateCloseToUtcNow()
    {
        using var service = new StudyGroupApiService();

        var request = new CreateStudyGroupRequest
        {
            Name = "Organic Squad",
            Subject = Subject.Chemistry
        };

        var before = DateTime.UtcNow;

        // Act
        var response = await service.CreateStudyGroupRawAsync(request);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<StudyGroup>();
        var after = DateTime.UtcNow;

        // Assert
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.CreateDate, Is.GreaterThanOrEqualTo(before));
        Assert.That(created.CreateDate, Is.LessThanOrEqualTo(after));
    }
    
    [Test]
    [Property("TCID", "TC-08")]
    public async Task JoinStudyGroup_ShouldReturnBadRequest_WhenUserAlreadyMember()
    {
        using var service = new StudyGroupApiService();

        using var scope = service.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StudyGroupDbContext>();

        var user = new User(0, "kenobi", "kenobi@example.com");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var math = await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Math Group",
            Subject = Subject.Math
        });
        
        var first = await service.JoinStudyGroupRawAsync(math!.StudyGroupId, user.UserId);
        first.EnsureSuccessStatusCode();
        
        var second = await service.JoinStudyGroupRawAsync(math.StudyGroupId, user.UserId);

        Assert.That((int)second.StatusCode, Is.EqualTo(400));
        var error = await second.Content.ReadAsStringAsync();
        StringAssert.Contains("already a member", error);
    }

    [Test]
    [Property("TCID", "TC-15")]
    public async Task LeaveStudyGroup_ShouldRemoveUserFromGroup()
    {
        using var service = new StudyGroupApiService();

        using var scope = service.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StudyGroupDbContext>();

        var user = new User(0, "Kenobi", "kenobi@example.com");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var math = await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Math Group",
            Subject = Subject.Math
        });

        // Join
        var joinResponse = await service.JoinStudyGroupRawAsync(math!.StudyGroupId, user.UserId);
        joinResponse.EnsureSuccessStatusCode();

        // Act: leave
        var leaveResponse = await service.LeaveStudyGroupRawAsync(math.StudyGroupId, user.UserId);
        leaveResponse.EnsureSuccessStatusCode();

        // Assert: user no longer in group
        var groupFromDb = await db.StudyGroups
            .Include(g => g.Users)
            .FirstAsync(g => g.StudyGroupId == math.StudyGroupId);

        Assert.That(groupFromDb.Users.Any(u => u.UserId == user.UserId), Is.False);
    }

    [Test]
    [Property("TCID", "TC-16")]
    public async Task LeaveStudyGroup_ShouldReturnBadRequest_WhenUserNotMember()
    {
        using var service = new StudyGroupApiService();

        using var scope = service.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StudyGroupDbContext>();

        var user = new User(0, "kenobi", "kenobi@example.com");
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var math = await service.CreateStudyGroupAsync(new CreateStudyGroupRequest
        {
            Name = "Math Group",
            Subject = Subject.Math
        });

        // User never joined this group

        var response = await service.LeaveStudyGroupRawAsync(math!.StudyGroupId, user.UserId);

        Assert.That((int)response.StatusCode, Is.EqualTo(400)); // BadRequest
        var error = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("not a member", error);
    }
}
