namespace Study.API.Data.Models;

    public class StudyGroup
    {
        public StudyGroup()
        {
            Users = new List<User>();
            CreateDate = DateTime.UtcNow;
        }

        public StudyGroup(int studyGroupId, string name, Subject subject, DateTime createDate, List<User> users)
        {
            ValidateName(name);
            
            StudyGroupId = studyGroupId;
            Name = name;
            Subject = subject;
            CreateDate = createDate;
            Users = users ?? new List<User>();
        }

        public int StudyGroupId { get; set; }

        public string Name { get; set; } = string.Empty;

        public Subject Subject { get; set; }

        public DateTime CreateDate { get; set; }

        public List<User> Users { get; set; }

        public void AddUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!Users.Any(u => u.UserId == user.UserId))
            {
                Users.Add(user);
            }
        }

        public void RemoveUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var existingUser = Users.FirstOrDefault(u => u.UserId == user.UserId);
            if (existingUser != null)
            {
                Users.Remove(existingUser);
            }
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required", nameof(name));
            
            if (name.Length < 5 || name.Length > 30)
                throw new ArgumentException("Name must be between 5 and 30 characters", nameof(name));
        }
    }