
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
        private readonly IMapper<Translation, TranslationDto> _translationMapper;   

        public ApiQuestionService(HttpClient http, IMapperFactory mapperFactory) : base(http)
        {
            _surveyQuestionMapper = mapperFactory.Get<SurveyQuestion, SurveyQuestionDto>();
        }


        #region GET Methods
        public async Task<SurveyQuestion?> GetQuestionByIdAsync(int id)
        {
            var dto = await _http.GetFromJsonAsync<SurveyQuestionDto>($"{_baseApi}/questions/{id}");

            var question = _surveyQuestionMapper.MapToEntity(dto);
            return question;
        }

        public async Task<List<SurveyQuestion>> GetQuestionsByVarNameAsync(string varname)
        {
            var dtos = await _http.GetFromJsonAsync<IEnumerable<SurveyQuestionDto>>($"{_baseApi}/questions/by-var/{varname}");

            var question = dtos.Select(x => _surveyQuestionMapper.MapToEntity(x));
            return question.ToList();
        }

        public async Task<List<SurveyQuestion>> GetQuestionsByRefVarNameAsync(string varname)
        {
            var dtos = await _http.GetFromJsonAsync<IEnumerable<SurveyQuestionDto>>($"{_baseApi}/questions/by-ref/{varname}");

            var question = dtos.Select(x => _surveyQuestionMapper.MapToEntity(x));
            return question.ToList();
        }

        public async Task<List<SurveyQuestion>> SearchQuestions(string searchTerm)
        {
            var dtos = await _http.GetFromJsonAsync<IEnumerable<SurveyQuestionDto>>($"{_baseApi}/questions/search?query={Uri.EscapeDataString(searchTerm)}");
            var questions = dtos.Select(x => _surveyQuestionMapper.MapToEntity(x));
            return questions.ToList();
        }
        #endregion


       

        #region POST Methods

        public async Task<int> AddQuestion(SurveyQuestion question)
        {
            var dto = _surveyQuestionMapper.MapToDto(question);
            var response = await _http.PostAsJsonAsync($"{_baseApi}/questions", dto);
            response.EnsureSuccessStatusCode();
            var createdDto = await response.Content.ReadFromJsonAsync<SurveyQuestionDto>();
            return createdDto.ID;
        }

        public async Task<bool> CreateTranslation(Translation translation)
        {
            var dto = _translationMapper.MapToDto(translation);
            var response = await _http.PostAsJsonAsync($"{_baseApi}/questions/{translation.QID}/translations", dto);
            response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode;
        }
        #endregion

        #region PUT Methods
        public async Task<int> UpdateQuestion(SurveyQuestion question)
        {
            var dto = _surveyQuestionMapper.MapToDto(question);
            var response = await _http.PutAsJsonAsync($"{_baseApi}/questions/{dto.ID}", dto);
            response.EnsureSuccessStatusCode();
            return dto.ID;
        }
        public async Task<bool> UpdateTranslation(Translation translation)
        {
            var dto = _translationMapper.MapToDto(translation);
            var response = await _http.PutAsJsonAsync($"{_baseApi}/questions/{translation.QID}/translations/{translation.ID}", dto);
            response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode;
        }

        #endregion

        #region PATCH
        public async Task<bool> UpdateQnums(IEnumerable<SurveyQuestion> questions)
        {
            var dtos = questions.Select(x=>_surveyQuestionMapper.MapToDto(x)).ToList();
            var response = await _http.PatchAsJsonAsync($"{_baseApi}/questions/question-order", dtos);
            var result = response.EnsureSuccessStatusCode();
            return result.IsSuccessStatusCode;
        }

        #endregion

        #region DELETE Methods

        public async Task<bool> DeleteQuestion(SurveyQuestion question)
        {
            var response = await _http.DeleteAsync($"{_baseApi}/questions/{question.ID}");
            var result = response.EnsureSuccessStatusCode();
            return result.IsSuccessStatusCode;
        }
        #endregion 
      
        
    }
}
