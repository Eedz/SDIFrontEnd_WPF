using ITC_Contracts;
using ITCLib;

namespace SDIFrontEnd_WPF.Mappings
{
    public class UserStateMapper : IMapper<SurveyUserState, SurveyUserStateDto>
    {
        public  SurveyUserState MapToEntity(SurveyUserStateDto dto)
        {
            return new SurveyUserState
            {
                SurveyUserStateID = dto.SurveyUserStateID,
                ID = dto.SurveyUserStateID,
                UserStateID = dto.UserStateID,
                State = new UserState(dto.UserStateID, dto.UserStateName)
            };
        }

        public SurveyUserStateDto MapToDto(SurveyUserState entity)
        {
            return new SurveyUserStateDto
            {
                SurveyUserStateID = entity.ID,
                UserStateID = entity.State.ID,
                UserStateName = entity.State.UserStateName
            };
        }

    }
}