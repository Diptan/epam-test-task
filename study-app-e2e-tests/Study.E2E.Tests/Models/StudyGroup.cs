namespace Study.E2E.Tests.Models;

public class StudyGroup
{
    public int StudyGroupId { get; set; }

    public string Name { get; set; } = string.Empty;

    public Subject Subject { get; set; }

    public DateTime CreateDate { get; set; }
    
    public List<User> Users { get; set; } = new();
}