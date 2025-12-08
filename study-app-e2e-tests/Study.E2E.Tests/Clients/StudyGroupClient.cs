using System.Net.Http.Json;
using Study.E2E.Tests.Models;

namespace Study.E2E.Tests.Clients;

public class StudyGroupClient : IDisposable
{
    private readonly HttpClient _httpClient;

    public StudyGroupClient()
    {
        var baseUrl = Environment.GetEnvironmentVariable("STUDY_APP_BASE_URL")
                      ?? "http://localhost:5017";

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/")
        };
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
    
    public async Task<HttpResponseMessage> CreateStudyGroupRawAsync(CreateStudyGroupRequest request)
    {
        return await _httpClient.PostAsJsonAsync("api/studygroup", request);
    }

    public async Task<StudyGroup?> CreateStudyGroupAsync(CreateStudyGroupRequest request)
    {
        var response = await CreateStudyGroupRawAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StudyGroup>();
    }

    public async Task<IReadOnlyList<StudyGroup>> GetStudyGroupsAsync(string? subject = null, string? sortOrder = null)
    {
        var url = "api/studygroup";
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(subject))
            queryParams.Add($"subject={Uri.EscapeDataString(subject)}");
        if (!string.IsNullOrWhiteSpace(sortOrder))
            queryParams.Add($"sortOrder={Uri.EscapeDataString(sortOrder)}");

        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var groups = await response.Content.ReadFromJsonAsync<List<StudyGroup>>();
        return groups ?? new List<StudyGroup>();
    }

    public async Task<StudyGroup?> GetStudyGroupByIdAsync(int studyGroupId)
    {
        var response = await _httpClient.GetAsync($"api/studygroup/{studyGroupId}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<StudyGroup>();
    }

    public async Task<HttpResponseMessage> JoinStudyGroupRawAsync(int studyGroupId, int userId)
    {
        var url = $"api/studygroup/{studyGroupId}/join/{userId}";
        return await _httpClient.PostAsync(url, content: null);
    }

    public async Task<HttpResponseMessage> LeaveStudyGroupRawAsync(int studyGroupId, int userId)
    {
        var url = $"api/studygroup/{studyGroupId}/leave/{userId}";
        return await _httpClient.PostAsync(url, content: null);
    }

    public async Task<HttpResponseMessage> DeleteStudyGroupAsync(int studyGroupId)
    {
        var url = $"api/studygroup/{studyGroupId}";
        return await _httpClient.DeleteAsync(url);
    }

    public async Task CleanupAllStudyGroupsAsync()
    {
        var groups = await GetStudyGroupsAsync();
        foreach (var group in groups)
        {
            await DeleteStudyGroupAsync(group.StudyGroupId);
        }
    }
}
