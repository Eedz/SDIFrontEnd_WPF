using ITC_Contracts;
using ITCLib;

namespace SDIFrontEnd_WPF.Mappings
{
    public class ModeMapper : IMapper<SurveyMode, SurveyModeDto>
    {
        public SurveyMode MapToEntity(SurveyModeDto dto)
        {
            return new SurveyMode
            {
                ID = dto.ID,
                Mode = dto.Mode,
                ModeAbbrev = dto.ModeAbbrev
            };
        }

        public SurveyModeDto MapToDto(SurveyMode entity)
        {
            return new SurveyModeDto
            {
                ID = entity.ID,
                Mode = entity.Mode,
                ModeAbbrev = entity.ModeAbbrev
            };
        }
    }
}