using System.Net.Http;
using System.Net.Http.Json;
using ITCLib;
using ITC_Contracts;

namespace SDIFrontEnd_WPF;
public class ApiSurveyService : ApiServiceBase, IApiSurveyService
{
    public ApiSurveyService(HttpClient http) : base (http)
    {
    }

    public async Task<List<Survey>> GetAllAsync()
    {
        var dtos = await _http.GetFromJsonAsync<List<SurveyDto>>("api/surveys");

        var surveys = dtos.Select(dtos => new Survey
        {
            SID = dtos.SID,
            Title = dtos.Title,
            WaveID = dtos.WaveID,
            SurveyCode = dtos.SurveyCode,
            SurveyCodePrefix = dtos.SurveyCodePrefix,
            CountryCode = dtos.CountryCode,
            WebName = dtos.WebName,
            EnglishRouting = dtos.EnglishRouting,
            Locked = dtos.Locked,
            ReRun = dtos.ReRun,
            HideSurvey = dtos.HideSurvey,
            NCT = dtos.NCT,
            Wave = dtos.Wave,
            CreationDate = dtos.CreationDate,
            Cohort = dtos.Cohort == null ? null : new SurveyCohort (),
            Mode = dtos.Mode == null ? null : new SurveyMode (),
        }).ToList();
        return surveys;
    }

    public async Task<Survey?> GetSurveyByIdAsync(int id)
    {
        var dto =  await _http.GetFromJsonAsync<SurveyDto>($"api/surveys/{id}");

        var survey = new Survey
        {
            SID = dto.SID,
            Title = dto.Title,
            WaveID = dto.WaveID,
            SurveyCode = dto.SurveyCode,
            SurveyCodePrefix = dto.SurveyCodePrefix,
            CountryCode = dto.CountryCode,
            WebName = dto.WebName,
            EnglishRouting = dto.EnglishRouting,
            Locked = dto.Locked,
            ReRun = dto.ReRun,
            HideSurvey = dto.HideSurvey,
            NCT = dto.NCT,
            Wave = dto.Wave,
            CreationDate = dto.CreationDate,
            Cohort = dto.Cohort == null ? null : new SurveyCohort(),
            Mode = dto.Mode == null ? null : new SurveyMode(),
        };
        return survey;
    }

    public async Task<List<StudyWave>> GetAllWavesAsync()
    {
        var waves = await _http.GetFromJsonAsync<List<StudyWave>>("api/waves");

        return waves ?? new List<StudyWave>();
    }

    public async Task<List<Study>> GetAllStudiesAsync()
    {
        var waves = await _http.GetFromJsonAsync<List<Study>>("api/studies");
        return waves ?? new List<Study>();
    }

    public async Task<List<SurveyQuestion>> FindQuestionsByRefVarName(string refvarname)
    {
        return new List<SurveyQuestion>();
    }

    public async Task<int> CreateAsync(Survey survey)
    {
        var response = await _http.PostAsJsonAsync("api/surveys", survey);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<Survey>();
        return created!.SID;
    }

    public async Task UpdateAsync(Survey survey)
    {
        var response = await _http.PutAsJsonAsync($"api/surveys/{survey.SID}", survey);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/surveys/{id}");
        response.EnsureSuccessStatusCode();
    }
}

