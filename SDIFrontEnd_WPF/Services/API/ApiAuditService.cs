using ITC_Contracts;
using ITCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using SDIFrontEnd_WPF.Mappings;
namespace SDIFrontEnd_WPF
{
    public class ApiAuditService : ApiServiceBase, IApiAuditService
    {
        IMapper<ChangedSurveyQuestion, ChangedSurveyQuestionDto> _mapper;
        IMapper<AuditWording, AuditWordingDto> _wordingMapper;
        

        public ApiAuditService(HttpClient http, IMapperFactory mapperFactory) : base(http)
        {
            _mapper = mapperFactory.Get <ChangedSurveyQuestion, ChangedSurveyQuestionDto>();
            _wordingMapper = mapperFactory.Get <AuditWording, AuditWordingDto>();
        }

        public async Task<List<string>> GetAuditSurveys()
        {
            var dtos = await _http.GetFromJsonAsync<List<string>>($"api/history/surveys");
            return dtos;
        }

        public async Task<List<VariableName>> GetAuditVarNames(string survey)
        {
            var dtos = await _http.GetFromJsonAsync<List<VariableNameDto>>($"api/history/varnames?survey={survey}");
            if (dtos == null)
                return new List<VariableName>();

            return dtos.Select(x=> new VariableName(x.VarName)).ToList();
        }

        public async Task<List<ChangedSurveyQuestion>> GetQuestionHistory(int qid)
        {
            var dtos = await _http.GetFromJsonAsync<List<ChangedSurveyQuestionDto>>($"api/history/questions/{qid}");
            var questions = dtos.Select(x => _mapper.MapToEntity(x)).ToList();
            return questions;
        }

        public async Task<List<AuditWording>> GetWordingHistory(string wordType, int wordID)
        {
            var dtos = await _http.GetFromJsonAsync<List<AuditWordingDto>>($"api/history/wordings?type={wordType}&id={wordID}");
            var questions = dtos.Select(x => _wordingMapper.MapToEntity(x)).ToList();
            return questions;
        }       
    }
}
