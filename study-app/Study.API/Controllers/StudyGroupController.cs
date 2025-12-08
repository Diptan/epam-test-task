using Microsoft.AspNetCore.Mvc;
using study_app.DTOs;
using Study.API.Data.Repositories.Contracts;
using Study.API.Data.Models;

[ApiController]
[Route("api/[controller]")]
public class StudyGroupController : ControllerBase
{
    private readonly IStudyGroupRepository _studyGroupRepository;

    public StudyGroupController(IStudyGroupRepository studyGroupRepository)
    {
        _studyGroupRepository = studyGroupRepository;
    }

    /// <summary>
        /// Creates a new study group. Only one study group per subject is allowed.
        /// Name must be between 5-30 characters. Valid subjects: Math, Chemistry, Physics.
        /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateStudyGroup([FromBody] CreateStudyGroupRequest request)
    {
        if (request == null)
            return BadRequest("Study group data is required.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Study group name is required.");

        if (request.Name.Length < 5 || request.Name.Length > 30)
            return BadRequest("Study group name must be between 5 and 30 characters.");

        if (!Enum.IsDefined(typeof(Subject), request.Subject))
            return BadRequest("Invalid subject. Valid subjects are: Math, Chemistry, Physics.");

        try
        {
            var studyGroup = new StudyGroup
            {
                Name = request.Name,
                Subject = request.Subject,
                Users = new List<User>()
            };

            var createdGroup = await _studyGroupRepository.CreateStudyGroup(studyGroup);
            return CreatedAtAction(
                nameof(GetStudyGroupById),
                new { id = createdGroup.StudyGroupId },
                createdGroup
            );
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                $"An error occurred while creating the study group: {ex.Message}"
            );
        }
    }

    /// <summary>
        /// Gets all study groups with optional filtering by subject and sorting by creation date.
        /// </summary>
        /// <param name="subject">Optional: Filter by subject (Math, Chemistry, Physics)</param>
        /// <param name="sortOrder">Optional: Sort by creation date - 'asc'/'oldest' for oldest first, 'desc'/'newest' for newest first (default)</param>
    [HttpGet]
    public async Task<IActionResult> GetStudyGroups(
        [FromQuery] string? subject = null,
        [FromQuery] string? sortOrder = null
    )
    {
        try
        {
            var studyGroups = await _studyGroupRepository.GetStudyGroups(subject, sortOrder);
            return Ok(studyGroups);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                $"An error occurred while retrieving study groups: {ex.Message}"
            );
        }
    }

    /// <summary>
        /// Gets a study group by ID
        /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetStudyGroupById(int id)
    {
        try
        {
            var studyGroup = await _studyGroupRepository.GetStudyGroupById(id);

            if (studyGroup == null)
                return NotFound($"Study group with ID {id} not found.");

            return Ok(studyGroup);
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                $"An error occurred while retrieving the study group: {ex.Message}"
            );
        }
    }

    /// <summary>
        /// Adds a user to a study group (user joins the group)
        /// </summary>
    [HttpPost("{studyGroupId:int}/join/{userId:int}")]
    public async Task<IActionResult> JoinStudyGroup(int studyGroupId, int userId)
    {
        try
        {
            await _studyGroupRepository.JoinStudyGroup(studyGroupId, userId);
            return Ok($"User {userId} successfully joined study group {studyGroupId}.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                $"An error occurred while joining the study group: {ex.Message}"
            );
        }
    }

    /// <summary>
        /// Removes a user from a study group (user leaves the group)
        /// </summary>
    [HttpPost("{studyGroupId:int}/leave/{userId:int}")]
    public async Task<IActionResult> LeaveStudyGroup(int studyGroupId, int userId)
    {
        try
        {
            await _studyGroupRepository.LeaveStudyGroup(studyGroupId, userId);
            return Ok($"User {userId} successfully left study group {studyGroupId}.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                $"An error occurred while leaving the study group: {ex.Message}"
            );
        }
    }

    /// <summary>
        /// Deletes a study group by ID
        /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteStudyGroup(int id)
    {
        try
        {
            var studyGroup = await _studyGroupRepository.GetStudyGroupById(id);
            if (studyGroup == null)
                return NotFound($"Study group with ID {id} not found.");

            await _studyGroupRepository.DeleteStudyGroup(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                $"An error occurred while deleting the study group: {ex.Message}"
            );
        }
    }
}
