namespace Study.API.Data.Models;

public class User
{
    public User()
    {
        StudyGroups = new List<StudyGroup>();
    }

    public User(int userId, string name, string email)
    {
        UserId = userId;
        Name = name;
        Email = email;
        StudyGroups = new List<StudyGroup>();
    }

    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public List<StudyGroup> StudyGroups { get; set; }
}