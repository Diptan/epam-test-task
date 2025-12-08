using Microsoft.EntityFrameworkCore;
using Study.API.Data.Repositories.Contracts;
using Study.API.Data.Models;

namespace Study.API.Data.Repositories
{
    public class StudyGroupRepository : IStudyGroupRepository
    {
        private readonly StudyGroupDbContext _context;

        public StudyGroupRepository(StudyGroupDbContext context)
        {
            _context = context;
        }

        public async Task<StudyGroup> CreateStudyGroup(StudyGroup studyGroup)
        {
            if (studyGroup == null)
                throw new ArgumentNullException(nameof(studyGroup));
            
            if (string.IsNullOrWhiteSpace(studyGroup.Name) || 
                studyGroup.Name.Length < 5 || 
                studyGroup.Name.Length > 30)
            {
                throw new ArgumentException("Study group name must be between 5 and 30 characters.");
            }
            
            if (await StudyGroupExistsForSubject(studyGroup.Subject))
            {
                throw new InvalidOperationException($"A study group for subject '{studyGroup.Subject}' already exists.");
            }

            studyGroup.CreateDate = DateTime.UtcNow;
            
            _context.StudyGroups.Add(studyGroup);
            await _context.SaveChangesAsync();
            
            return studyGroup;
        }

        public async Task<IEnumerable<StudyGroup>> GetStudyGroups(string? subject = null, string? sortOrder = null)
        {
            IQueryable<StudyGroup> query = _context.StudyGroups.Include(sg => sg.Users);
            
            if (!string.IsNullOrWhiteSpace(subject))
            {
                if (Enum.TryParse<Subject>(subject, ignoreCase: true, out var subjectEnum))
                {
                    query = query.Where(sg => sg.Subject == subjectEnum);
                }
                else
                {
                    throw new ArgumentException($"Invalid subject '{subject}'. Valid subjects are: Math, Chemistry, Physics.");
                }
            }
            
            query = sortOrder?.ToLower() switch
            {
                "asc" or "oldest" => query.OrderBy(sg => sg.CreateDate),
                "desc" or "newest" => query.OrderByDescending(sg => sg.CreateDate),
                null => query.OrderByDescending(sg => sg.CreateDate), // Default to newest first
                _ => query.OrderByDescending(sg => sg.CreateDate)
            };
            var gg = await query.ToListAsync();
            return await query.ToListAsync();
        }

        public async Task<StudyGroup?> GetStudyGroupById(int studyGroupId)
        {
            return await _context.StudyGroups
                .Include(sg => sg.Users)
                .FirstOrDefaultAsync(sg => sg.StudyGroupId == studyGroupId);
        }

        public async Task<StudyGroup?> GetStudyGroupBySubject(Subject subject)
        {
            return await _context.StudyGroups
                .Include(sg => sg.Users)
                .FirstOrDefaultAsync(sg => sg.Subject == subject);
        }

        public async Task<bool> StudyGroupExistsForSubject(Subject subject)
        {
            return await _context.StudyGroups.AnyAsync(sg => sg.Subject == subject);
        }

        public async Task JoinStudyGroup(int studyGroupId, int userId)
        {
            var studyGroup = await _context.StudyGroups
                .Include(sg => sg.Users)
                .FirstOrDefaultAsync(sg => sg.StudyGroupId == studyGroupId);

            if (studyGroup == null)
                throw new KeyNotFoundException($"Study group with ID {studyGroupId} not found.");

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            
            if (studyGroup.Users.Any(u => u.UserId == userId))
                throw new InvalidOperationException($"User {userId} is already a member of study group {studyGroupId}.");

            studyGroup.AddUser(user);
            await _context.SaveChangesAsync();
        }

        public async Task LeaveStudyGroup(int studyGroupId, int userId)
        {
            var studyGroup = await _context.StudyGroups
                .Include(sg => sg.Users)
                .FirstOrDefaultAsync(sg => sg.StudyGroupId == studyGroupId);

            if (studyGroup == null)
                throw new KeyNotFoundException($"Study group with ID {studyGroupId} not found.");

            var user = studyGroup.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
                throw new InvalidOperationException($"User {userId} is not a member of study group {studyGroupId}.");

            studyGroup.RemoveUser(user);
            await _context.SaveChangesAsync();
        }
    }
}