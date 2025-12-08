namespace Study.E2E.Tests.Models;

public class CreateStudyGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public Subject Subject { get; set; }
}