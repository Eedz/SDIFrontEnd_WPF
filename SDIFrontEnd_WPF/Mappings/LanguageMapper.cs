using ITC_Contracts;
using ITCLib;

namespace SDIFrontEnd_WPF.Mappings
{
    public class LanguageMapper : IMapper<SurveyLanguage, SurveyLanguageDto>
    {
        public SurveyLanguageDto MapToDto(SurveyLanguage entity)
        {
            return new SurveyLanguageDto
            {
                SurveyLanguageID = entity.SurveyLanguageID,
                LanguageID = entity.ID,
                LanguageName = entity.SurvLanguage.LanguageName
            };
        }



        public SurveyLanguage MapToEntity(SurveyLanguageDto dto)
        {
            return new SurveyLanguage
            {
                SurveyLanguageID = dto.SurveyLanguageID,
                ID = dto.LanguageID,

                SurvLanguage = new Language() { ID = dto.LanguageID, LanguageName = dto.LanguageName ?? string.Empty }
            };
        }
    }
}