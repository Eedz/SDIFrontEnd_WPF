using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using ITC_Contracts;
using ITCLib;
using System.Net.Http;
using System.Net.Http.Json;
using SDIFrontEnd_WPF.Mappings;
namespace SDIFrontEnd_WPF;
public class ApiSurveyService : ApiServiceBase, IApiSurveyService
{
    SurveyMapper surveyMapper;
    SurveyQuestionMapper questionMapper;

    public ApiSurveyService(HttpClient http, SurveyMapper surveymapper, SurveyQuestionMapper questionmapper) : base (http)
    {
        this.surveyMapper = surveymapper;
        questionMapper = questionmapper;

    }

    public async Task<List<Survey>> GetAllAsync()
    {
        var dtos = await _http.GetFromJsonAsync<List<SurveyDto>>("api/surveys");

        var surveys = dtos.Select(dtos => surveyMapper.MapToEntity(dtos)).ToList();
        
        return surveys;
    }

    public async Task<Survey?> GetSurveyByIdAsync(int id)
    {
        var dto =  await _http.GetFromJsonAsync<SurveyDto>($"api/surveys/{id}");

        var survey = surveyMapper.MapToEntity(dto);
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

    public async Task<List<SurveyQuestion>> GetSurveyQuestions(int id)
    {
        var dto = await _http.GetFromJsonAsync<SurveyDto>($"api/surveys/{id}/questions");
        if (dto == null) return new List<SurveyQuestion>();
        var questions = dto.Questions.Select(x => questionMapper.MapToEntity(x));
        return questions.ToList() ?? new List<SurveyQuestion>();
    }

    public async Task<List<DeletedQuestion>> GetDeletedQuestions(int id)
    {
        var dto = await _http.GetFromJsonAsync<List<DeletedQuestion>>($"api/surveys/{id}/deleted");
        if (dto == null) return new List<DeletedQuestion>();
        var deletedQuestions = dto.Select(x=> new DeletedQuestion()
        {
            ID = x.ID,
            SurveyCode = x.SurveyCode,
            VarName = x.VarName,
            VarLabel = x.VarLabel,
            DomainLabel = x.DomainLabel,
            TopicLabel = x.TopicLabel,
            ContentLabel = x.ContentLabel,
            ProductLabel = x.ProductLabel,
            DeleteDate = x.DeleteDate,
            DeletedBy = x.DeletedBy,
            DeleteNotes = x.DeleteNotes
        });
        return deletedQuestions.ToList();
    }


    public async Task<List<Survey>> GetChangedSurveys(DateTime date)
    {
        string formattedDate = date.ToString("yyyy-MM-dd");
        var dto = await _http.GetFromJsonAsync<List<SurveyDto>>($"api/surveys/changed?date={formattedDate}");
        var surveys = dto.Select(dtos => new Survey
        {
            SID = dtos.SID,
            SurveyCode = dtos.SurveyCode,
            Locked = dtos.Locked
        }).ToList();
        return surveys;
    }

    public async Task<List<Survey>> GetSurveysByVar(string varname)
    {
        var dto = await _http.GetFromJsonAsync<List<SurveyDto>>($"api/surveys/by-var/{varname}");
        var surveys = dto.Select(dtos => new Survey
        {
            SID = dtos.SID,
            SurveyCode = dtos.SurveyCode,
            Locked = dtos.Locked
        }).ToList();
        return surveys;
    }

    public async Task<List<Survey>> GetSurveysByRefVar(string varname)
    {
        var dto = await _http.GetFromJsonAsync<List<SurveyDto>>($"api/surveys/by-ref/{varname}");
        var surveys = dto.Select(dtos => new Survey
        {
            SID = dtos.SID,
            SurveyCode = dtos.SurveyCode,
            Locked = dtos.Locked
        }).ToList();
        return surveys;
    }

    public async Task<int> CreateAsync(Survey survey)
    {
        var response = await _http.PostAsJsonAsync("api/surveys", survey);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<Survey>();
        return created!.SID;
    }

    public async Task<Survey> UpdateSurvey(Survey survey)
    {
        var dto = surveyMapper.MapToDto(survey);

        var response = await _http.PutAsJsonAsync($"api/surveys/{dto.SID}", dto);
        response.EnsureSuccessStatusCode();
        return survey;
    }

    public async Task DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/surveys/{id}");
        response.EnsureSuccessStatusCode();
    }
}

