using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;
using ITC_Contracts;
using System.Printing;
namespace SDIFrontEnd_WPF.Mappings
{
    public class SurveyMapper : IMapper<Survey, SurveyDto>
    {
        private readonly IMapper<SurveyCohort, SurveyCohortDto> cohortMapper;
        private readonly IMapper<SurveyMode, SurveyModeDto> modeMapper;
        private readonly IMapper<SurveyUserState, SurveyUserStateDto> userStateMapper;
        private readonly IMapper<SurveyLanguage, SurveyLanguageDto> languageMapper;
        private readonly IMapper<SurveyScreenedProduct, SurveyScreenedProductDto> screenedProductMapper;

        public SurveyMapper(IMapperFactory mapperFactory)
        {
            this.cohortMapper = mapperFactory.Get<SurveyCohort, SurveyCohortDto>();
            this.modeMapper = mapperFactory.Get<SurveyMode, SurveyModeDto>();
            this.userStateMapper = mapperFactory.Get<SurveyUserState, SurveyUserStateDto>();
            this.languageMapper = mapperFactory.Get<SurveyLanguage, SurveyLanguageDto>();
            this.screenedProductMapper = mapperFactory.Get<SurveyScreenedProduct, SurveyScreenedProductDto>();
        }

        public SurveyDto MapToDto(Survey survey)
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
                Cohort = cohortMapper.MapToDto(survey.Cohort),
                Mode = modeMapper.MapToDto(survey.Mode),
                UserStates = survey.UserStates.Select(x => userStateMapper.MapToDto(x)).ToList(),
                Languages = survey.LanguageList.Select(x => languageMapper.MapToDto(x)).ToList(),
                Products = survey.ScreenedProducts.Select(x => screenedProductMapper.MapToDto(x)).ToList(),
            };
        }

        public Survey MapToEntity(SurveyDto dto)
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
                Cohort = dto.Cohort == null ? new SurveyCohort() : cohortMapper.MapToEntity(dto.Cohort),
                Mode = dto.Mode == null ? new SurveyMode() : modeMapper.MapToEntity(dto.Mode),
                UserStates = dto.UserStates.Select(x => userStateMapper.MapToEntity(x)).ToList(),
                LanguageList = dto.Languages.Select(x => languageMapper.MapToEntity(x)).ToList(),
                ScreenedProducts = dto.Products.Select(x => screenedProductMapper.MapToEntity(x)).ToList(),
            };
        }

        public void MapToExisting(SurveyDto dto, Survey survey)
        {

        }
    }
}
