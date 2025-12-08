using NUnit.Framework.Legacy;
using Study.API.Data.Models;

namespace Study.Unit.Tests
{
    [TestFixture]
    public class StudyGroupDomainTests
    {
        [Test]
        [Property("TCID", "TC-03")]
        public void Constructor_ShouldThrow_WhenNameShorterThan5Chars()
        {
            // Arrange
            var shortName = "Math";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new StudyGroup(1, shortName, Subject.Math, DateTime.UtcNow, null));

            StringAssert.Contains("between 5 and 30 characters", ex.Message);
        }

        [Test]
        [Property("TCID", "TC-04")]
        public void Constructor_ShouldThrow_WhenNameLongerThan30Chars()
        {
            // Arrange
            var longName = "ThisNameIsWayTooLongForGroup!!!";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new StudyGroup(1, longName, Subject.Math, DateTime.UtcNow, null));

            StringAssert.Contains("between 5 and 30 characters", ex.Message);
        }

        [Test]
        [Property("TCID", "TC-01")]
        [TestCase(5, "Short")]
        [TestCase(15, "AlgebraMasterss")]
        [TestCase(29, "ABCDEFGHIJKLMNOPQRSTUVWXYZABC")]
        public void Constructor_ShouldCreateValidStudyGroup_WhenNameWithinLimits(int length, string validName)
        {
            // Arrange
            var subject = Subject.Math;

            // Act
            var group = new StudyGroup(1, validName, subject, DateTime.UtcNow, null);

            // Assert
            Assert.That(group.Name, Is.EqualTo(validName));
            Assert.That(group.Subject, Is.EqualTo(subject));
            Assert.That(group.Users, Is.Not.Null);
            Assert.That(group.Name.Length, Is.EqualTo(length));
        }

        [Test]
        [Property("TCID", "TC-08")]
        public void AddUser_ShouldAddUser_WhenNotAlreadyMember()
        {
            // Arrange
            var group = new StudyGroup(1, "Algebra Wizards", Subject.Math, DateTime.UtcNow, null);
            var user = new User(10, "Alice", "alice@example.com");

            // Act
            group.AddUser(user);

            // Assert
            Assert.That(group.Users.Count, Is.EqualTo(1));
            Assert.That(group.Users.Single().UserId, Is.EqualTo(10));
        }

        [Test]
        [Property("TCID", "TC-08")]
        public void AddUser_ShouldNotAddDuplicateUser()
        {
            // Arrange
            var group = new StudyGroup(1, "Algebra Wizards", Subject.Math, DateTime.UtcNow, null);
            var user = new User(10, "Alice", "alice@example.com");

            group.AddUser(user);

            // Act
            group.AddUser(user);

            // Assert
            Assert.That(group.Users.Count, Is.EqualTo(1));
        }

        [Test]
        [Property("TCID", "TC-16")]
        public void RemoveUser_ShouldRemoveExistingUser()
        {
            // Arrange
            var group = new StudyGroup(1, "Algebra Wizards", Subject.Math, DateTime.UtcNow, null);
            var user = new User(10, "Alice", "alice@example.com");
            group.AddUser(user);

            // Act
            group.RemoveUser(user);

            // Assert
            Assert.That(group.Users, Is.Empty);
        }

        [Test]
        [Property("TCID", "TC-16")]
        public void RemoveUser_ShouldDoNothing_WhenUserNotMember()
        {
            // Arrange
            var group = new StudyGroup(1, "Algebra Wizards", Subject.Math, DateTime.UtcNow, null);
            var member = new User(10, "Alice", "alice@example.com");
            var nonMember = new User(20, "Bob", "bob@example.com");
            group.AddUser(member);

            // Act
            group.RemoveUser(nonMember);

            // Assert
            Assert.That(group.Users.Count, Is.EqualTo(1));
            Assert.That(group.Users.Single().UserId, Is.EqualTo(10));
        }
    }
}