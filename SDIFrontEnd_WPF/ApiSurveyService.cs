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

    public async Task<List<SurveyQuestion>> GetSurveyQuestions(int id)
    {
        var dto = await _http.GetFromJsonAsync<SurveyDto>($"api/surveys/{id}/questions");
        if (dto == null) return new List<SurveyQuestion>();
        var questions = dto.Questions.Select(x =>MapToEntity(x));
        return questions.ToList() ?? new List<SurveyQuestion>();
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

    //private SurveyQuestionDto MapToDto(SurveyQuestion entity)
    //{
    //    return new SurveyQuestionDto
    //    {
    //        VarName = entity.VarName,
    //        VarLabel = entity.VarLabel,
    //        Domain = new VarNameLabelDto() { LabelText = entity.Domain.LabelText, ID = entity.Domain.ID },
    //        Topic = new VarNameLabelDto() { LabelText = entity.Topic.LabelText, ID = entity.Topic.ID },
    //        Content = new VarNameLabelDto() { LabelText = entity.Content.LabelText, ID = entity.Content.ID },
    //        Product = new VarNameLabelDto() { LabelText = entity.Product.LabelText, ID = entity.Product.ID },
    //    };
    //}

    private SurveyQuestion MapToEntity(SurveyQuestionDto dto)
    {
        return new SurveyQuestion
        {
            VarName = new VariableName(dto.VarName.VarName)
            {
                VarLabel = dto.VarName.VarLabel,
                Domain = new DomainLabel() { LabelText = dto.VarName.Domain.LabelText, ID = dto.VarName.Domain.ID },
                Topic = new TopicLabel() { LabelText = dto.VarName.Topic.LabelText, ID = dto.VarName.Topic.ID },
                Content = new ContentLabel() { LabelText = dto.VarName.Content.LabelText, ID = dto.VarName.Content.ID },
                Product = new ProductLabel() { LabelText = dto.VarName.Product.LabelText, ID = dto.VarName.Product.ID },
            },
            Qnum = dto.Qnum,
            AltQnum = dto.AltQnum,
            PrePW = dto.PrePW == null ? null : new Wording() { WordID = dto.PrePW.ID, WordingText = dto.PrePW.WordingText },
            PreIW = dto.PreIW == null ? null : new Wording() { WordID = dto.PreIW.ID, WordingText = dto.PreIW.WordingText },
            PreAW = dto.PreAW == null ? null : new Wording() { WordID = dto.PreAW.ID, WordingText = dto.PreAW.WordingText },
            LitQW = dto.LitQW == null ? null : new Wording() { WordID = dto.LitQW.ID, WordingText = dto.LitQW.WordingText },
            PstIW = dto.PstIW == null ? null : new Wording() { WordID = dto.PstIW.ID, WordingText = dto.PstIW.WordingText },
            PstPW = dto.PstPW == null ? null : new Wording() { WordID = dto.PstPW.ID, WordingText = dto.PstPW.WordingText },
            RespOptionsS = dto.RespOptionsS == null ? null : new ResponseSet() { RespSetName = dto.RespOptionsS.RespSetName, RespList = dto.RespOptionsS.RespList },
            NRCodesS = dto.NRCodesS == null ? null : new ResponseSet() { RespSetName = dto.NRCodesS.RespSetName, RespList = dto.NRCodesS.RespList },



        };
    }
}

