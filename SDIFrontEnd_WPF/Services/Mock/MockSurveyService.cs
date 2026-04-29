
using ITC_Contracts;
using ITCLib;
using System.Text.Json;

namespace SDIFrontEnd_WPF;
public class MockSurveyService : IApiSurveyService
{
    public MockSurveyService() 
    {
    }

    public async Task<bool> CheckHealthAsync()
    {
        return true;
    }

    public async Task<List<Survey>> GetAllAsync()
    {
        var surveys = new List<Survey>
        {
            new Survey { SID = 1, SurveyCode = "SURV001", Locked = false },
            new Survey { SID = 2, SurveyCode = "SURV002", Locked = true },
            new Survey { SID = 3, SurveyCode = "SURV003", Locked = false }
        };

        return surveys;
    }

    public async Task<Survey?> GetSurveyByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<List<StudyWave>> GetAllWavesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<List<Study>> GetAllStudiesAsync()
    {
        var json = await System.IO.File.ReadAllTextAsync("MockData/Studies.json");
        var studies = JsonSerializer.Deserialize<List<Study>>(json);
        return studies;
    }

    public async Task<List<SurveyQuestion>> FindQuestionsByRefVarName(string refvarname)
    {
        return new List<SurveyQuestion>();
    }

    public async Task<List<SurveyQuestion>> GetSurveyQuestions(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<List<DeletedQuestion>> GetDeletedQuestions(int id)
    {
        throw new NotImplementedException();
    }


    public async Task<List<Survey>> GetChangedSurveys(DateTime date)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Survey>> GetSurveysByVar(string varname)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Survey>> GetSurveysByRefVar(string varname)
    {
        throw new NotImplementedException();
    }

    public async Task<int> CreateAsync(Survey survey)
    {
        throw new NotImplementedException();
    }

    public async Task<Survey> UpdateSurvey(Survey survey)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
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

    private Survey MapToEntity(SurveyDto dto)
    {
        return new Survey
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
            Cohort = dto.Cohort == null ? new SurveyCohort() : MapToEntity(dto.Cohort),
            Mode = dto.Mode == null ? new SurveyMode() : MapToEntity(dto.Mode),
            UserStates = dto.UserStates.Select(x => MapToEntity(x)).ToList(),
            LanguageList = dto.Languages.Select(x => MapToEntity(x)).ToList(),
            ScreenedProducts = dto.Products.Select(x => MapToEntity(x)).ToList(),
        };
    }

    private SurveyUserState MapToEntity(SurveyUserStateDto dto)
    {
        return new SurveyUserState
        {
            SurveyUserStateID = dto.SurveyUserStateID,
            ID = dto.UserStateID,
            UserStateID = dto.UserStateID,
            State = new UserState(dto.UserStateID, dto.UserStateName)
        };
    }

    private SurveyUserStateDto MapToDto(SurveyUserState entity)
    {
        return new SurveyUserStateDto
        {
            SurveyUserStateID = entity.SurveyUserStateID,
            UserStateID = entity.UserStateID,
            UserStateName = entity.State.UserStateName
        };
    }

    private SurveyCohort MapToEntity(SurveyCohortDto dto)
    {
        return new SurveyCohort
        {
            ID = dto.ID,
            Cohort = dto.Cohort,
            Code = dto.Code,
            WebName = dto.WebName
        };
    }

    private SurveyCohortDto MapToDto(SurveyCohort entity)
    {
        return new SurveyCohortDto
        {
            ID = entity.ID,
            Cohort = entity.Cohort,
            Code = entity.Code,
            WebName = entity.WebName
        };
    }

    private SurveyMode MapToEntity(SurveyModeDto dto)
    {
        return new SurveyMode
        {
            ID = dto.ID,
            Mode = dto.Mode,
            ModeAbbrev = dto.ModeAbbrev
        };
    }

    private SurveyModeDto MapToDto(SurveyMode entity)
    {
        return new SurveyModeDto
        {
            ID = entity.ID,
            Mode = entity.Mode,
            ModeAbbrev = entity.ModeAbbrev
        };
    }

    private SurveyLanguageDto MapToDto (SurveyLanguage entity)
    {
        return new SurveyLanguageDto
        {
            SurveyLanguageID = entity.SurveyLanguageID,
            LanguageID = entity.ID,
            LanguageName = entity.SurvLanguage.LanguageName
        };
    }

    private SurveyScreenedProductDto MapToDto(SurveyScreenedProduct entity)
    {
        return new SurveyScreenedProductDto
        {
            SurveyScreenedProductID = entity.SurveyScreenedProductID,
            ScreenedProductID = entity.ID,
            ScreenedProductName = entity.Product.ProductName
        };
    }

    private SurveyLanguage MapToEntity(SurveyLanguageDto dto)
    {
        return new SurveyLanguage
        {
            SurveyLanguageID = dto.SurveyLanguageID,
            ID = dto.LanguageID,
            
            SurvLanguage = new Language() { ID = dto.LanguageID, LanguageName = dto.LanguageName ?? string.Empty }
        };
    }

    private SurveyScreenedProduct MapToEntity(SurveyScreenedProductDto dto)
    {
        return new SurveyScreenedProduct
        {
            SurveyScreenedProductID = dto.SurveyScreenedProductID,
            ID = dto.ScreenedProductID,
            Product = new ScreenedProduct() { ID = dto.ScreenedProductID, ProductName = dto.ScreenedProductName ?? string.Empty }
        };
    }

    private SurveyDto MapToDto(Survey survey)
    {
        return new SurveyDto
        {
            SID = survey.SID,
            Title = survey.Title,
            WaveID = survey.WaveID,
            SurveyCode = survey.SurveyCode,
            SurveyCodePrefix = survey.SurveyCodePrefix,
            CountryCode = survey.CountryCode,
            WebName = survey.WebName,
            EnglishRouting = survey.EnglishRouting,
            Locked = survey.Locked,
            ReRun = survey.ReRun,
            HideSurvey = survey.HideSurvey,
            NCT = survey.NCT,
            Wave = survey.Wave,
            CreationDate = survey.CreationDate,
            Cohort = MapToDto(survey.Cohort),
            Mode = MapToDto(survey.Mode),
            UserStates = survey.UserStates.Select(x => MapToDto(x)).ToList(),
            Languages = survey.LanguageList.Select(x => MapToDto(x)).ToList(),
            Products = survey.ScreenedProducts.Select(x => MapToDto(x)).ToList(),
        };
        
    }

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

    public Task<bool> UpdateTranslation(Translation translation)
    {
        throw new NotImplementedException();
    }

    public Task<bool> InsertTranslation(Translation translation)
    {
        throw new NotImplementedException();
    }

    
}

