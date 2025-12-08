using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Study.API.Data.Models;
using Study.API.Data.Repositories.Contracts;
using study_app.DTOs;

namespace Study.Tests.Unit
{
    [TestFixture]
    public class StudyGroupControllerUnitTests
    {
        private Mock<IStudyGroupRepository> _repoMock = null!;
        private StudyGroupController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IStudyGroupRepository>();
            _controller = new StudyGroupController(_repoMock.Object);
        }

        [Test]
        [Property("TCID", "TC-05")]
        public async Task CreateStudyGroup_ShouldReturnBadRequest_WhenSubjectInvalidEnum()
        {
            // Arrange
            var request = new CreateStudyGroupRequest
            {
                Name = "Valid Name",
                Subject = (Subject)999 // invalid enum value
            };

            // Act
            var result = await _controller.CreateStudyGroup(request);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            Assert.That(badRequest != null, "Expected BadRequestObjectResult for invalid subject.");
            StringAssert.Contains("Invalid subject", badRequest!.Value!.ToString());

            // Ensure repository is NOT called
            _repoMock.Verify(r => r.CreateStudyGroup(It.IsAny<StudyGroup>()), Times.Never);
        }
    }
}