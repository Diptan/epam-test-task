using System.Net.Http.Json;
using study_app.DTOs;
using Study.API.Data.Models;

namespace Study.Integration.Tests.Infrastructure;

public class StudyGroupApiService : IDisposable
{
    private readonly StudyGroupApiFactory _factory;
    private readonly HttpClient _httpClient;

    public IServiceProvider Services => _factory.Services;

    public StudyGroupApiService()
    {
        _factory = new StudyGroupApiFactory();
        _httpClient = _factory.CreateClient();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _factory?.Dispose();
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
}
