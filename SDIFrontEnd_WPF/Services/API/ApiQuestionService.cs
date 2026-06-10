
using ITC_Contracts;
using ITCLib;
using SDIFrontEnd_WPF.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    public class ApiQuestionService : ApiServiceBase, IApiQuestionService
    {
        private readonly IMapper<SurveyQuestion, SurveyQuestionDto> _surveyQuestionMapper;

        public ApiQuestionService(HttpClient http, IMapperFactory mapperFactory) : base(http)
        {
            _surveyQuestionMapper = mapperFactory.Get<SurveyQuestion, SurveyQuestionDto>();
        }

        public async Task<SurveyQuestion?> GetQuestionByIdAsync(int id)
        {
            var dto = await _http.GetFromJsonAsync<SurveyQuestionDto>($"api/questions/{id}");

            var question = _surveyQuestionMapper.MapToEntity(dto);
            return question;
        }

        public async Task<List<SurveyQuestion>> GetQuestionsByVarNameAsync(string varname)
        {
            var dtos = await _http.GetFromJsonAsync<IEnumerable<SurveyQuestionDto>>($"api/questions/by-var/{varname}");

            var question = dtos.Select(x => _surveyQuestionMapper.MapToEntity(x));
            return question.ToList();
        }

        public async Task<List<SurveyQuestion>> GetQuestionsByRefVarNameAsync(string varname)
        {
            var dtos = await _http.GetFromJsonAsync<IEnumerable<SurveyQuestionDto>>($"api/questions/by-ref/{varname}");

            var question = dtos.Select(x => _surveyQuestionMapper.MapToEntity(x));
            return question.ToList();
        }

        public async Task<SurveyQuestionRecord> SaveQuestion(SurveyQuestionRecord question)
        {
            var dto = _surveyQuestionMapper.MapToDto(question.Item);

            HttpResponseMessage response;
            if (question.NewRecord)
                response = await _http.PostAsJsonAsync($"api/questions", dto);
            else if (question.Deleted)
                response = await _http.DeleteAsync($"api/questions/{dto.ID}");
            else if (question.ShouldSave)
            {
                if (question.DirtyLabels)
                    await _http.PutAsJsonAsync($"api/labels/variable/{dto.VarName.VarName}", dto.VarName);
                response = await _http.PutAsJsonAsync($"api/questions/{dto.ID}", dto);
            }
            else
                return question; // No changes to save

            response.EnsureSuccessStatusCode();
            return question;
        }

        public async Task<List<SurveyQuestion>> SearchQuestions(string searchTerm)
        {
            var dtos = await _http.GetFromJsonAsync<IEnumerable<SurveyQuestionDto>>($"api/questions/search?query={Uri.EscapeDataString(searchTerm)}");           
            var questions = dtos.Select(x => _surveyQuestionMapper.MapToEntity(x));
            return questions.ToList();
        }

        public async Task<int> AddQuestion(SurveyQuestion question)
        {
            var dto = _surveyQuestionMapper.MapToDto(question);
            var response = await _http.PostAsJsonAsync($"api/questions", dto);
            response.EnsureSuccessStatusCode();
            var createdDto = await response.Content.ReadFromJsonAsync<SurveyQuestionDto>();
            return createdDto.ID;
        }

        public async Task<int> UpdateQuestion(SurveyQuestion question)
        {
            var dto = _surveyQuestionMapper.MapToDto(question);
            var response = await _http.PutAsJsonAsync($"api/questions/{dto.ID}", dto);
            response.EnsureSuccessStatusCode();
            return dto.ID;
        }

        public async Task<bool> UpdateQnums(IEnumerable<SurveyQuestion> questions)
        {
            var dtos = questions.Select(x=>_surveyQuestionMapper.MapToDto(x));
            var response = await _http.PatchAsJsonAsync($"api/questions/question-order", dtos);
            var result = response.EnsureSuccessStatusCode();
            return result.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteQuestion(SurveyQuestion question)
        {
            var response = await _http.DeleteAsync($"api/questions/{question.ID}");
            var result = response.EnsureSuccessStatusCode();
            return result.IsSuccessStatusCode;
        }       
    }
}
