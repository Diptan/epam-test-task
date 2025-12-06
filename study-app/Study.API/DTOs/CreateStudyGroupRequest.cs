using Study.API.Data.Models;

namespace study_app.DTOs;

public class CreateStudyGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public Subject Subject { get; set; }
}