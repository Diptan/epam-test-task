using Study.API.Data.Models;

namespace Study.API.Data.Repositories.Contracts;
    
public interface IStudyGroupRepository
{
    Task<StudyGroup> CreateStudyGroup(StudyGroup studyGroup);
    Task<IEnumerable<StudyGroup>> GetStudyGroups(string? subject = null, string? sortOrder = null);
    Task<StudyGroup?> GetStudyGroupById(int studyGroupId);
    Task<StudyGroup?> GetStudyGroupBySubject(Subject subject);
    Task<bool> StudyGroupExistsForSubject(Subject subject);
    Task JoinStudyGroup(int studyGroupId, int userId);
    Task LeaveStudyGroup(int studyGroupId, int userId);
}
