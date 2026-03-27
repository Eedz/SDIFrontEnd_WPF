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
    public class ApiPraccingService : ApiServiceBase, IApiPraccingService
    {
        PraccingIssueMapper _mapper;
        PraccingResponseMapper _responseMapper;
        

        public ApiPraccingService(HttpClient httpClient, PraccingIssueMapper praccingMapper, PraccingResponseMapper responseMapper) : base(httpClient)
        {
            _mapper = praccingMapper;
            _responseMapper = responseMapper;
        }

        public async Task<int> DeletePraccingIssue(int id)
        {
            var result = await _http.DeleteAsync($"api/praccing/{id}");
            result.EnsureSuccessStatusCode();
            return 0;
            
        }

        public async Task<List<PraccingCategory>> GetPraccingCategories()
        {
            var result = await _http.GetFromJsonAsync<List<PraccingCategoryDto>>($"api/praccing/categories");
            
            if (result ==null)
                return new List<PraccingCategory>();

            return result.Select(x=>new PraccingCategory()
            {
                ID = x.ID,
                Category = x.Category,
                
            }).ToList();
        }

        public async Task<List<PraccingIssue>> GetPraccingIssues(int surveyid)
        {
            var result = await _http.GetFromJsonAsync<List<PraccingIssueDto>>($"api/praccing?id={surveyid}");

            if (result == null)
                return new List<PraccingIssue>();

            return result.Select(x => _mapper.MapToEntity(x)).ToList();
        }

        public async Task<PraccingIssue> AddPraccingIssue(PraccingIssue issue)
        {
            var dto = _mapper.MapToDto(issue);
            var response = await _http.PostAsJsonAsync("api/praccing", dto);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<PraccingIssueDto>();
            if (created == null)
                return null;
            var createdEntity = _mapper.MapToEntity(created);
            return createdEntity;
        }

        public async Task<PraccingIssue> UpdatePraccingIssue(PraccingIssue issue)
        {
            var dto = _mapper.MapToDto(issue);
            var response = await _http.PutAsJsonAsync("api/praccing", dto);

            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<PraccingIssueDto>();
            if (created == null)
                return null;
            var createdEntity = _mapper.MapToEntity(created);
            return createdEntity;
        }

        public async Task<PraccingResponse> AddPraccingResponse(PraccingResponse issueresponse)
        {
            var dto = _responseMapper.MapToDto(issueresponse);
            var response = await _http.PostAsJsonAsync("api/praccing/responses", dto);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<PraccingResponseDto>();
            if (created == null)
                return null;
            var createdEntity = _responseMapper.MapToEntity(created);
            return createdEntity;
        }

        public async Task<int> DeletePraccingResponse(int id)
        {
            var response = await _http.DeleteAsync($"api/praccing/responses/{id}");
            response.EnsureSuccessStatusCode();
            return 0;
        }

        public async Task<PraccingResponse> UpdatePraccingResponse(PraccingResponse issueresponse)
        {
            var dto = _responseMapper.MapToDto(issueresponse);
            var response = await _http.PutAsJsonAsync("api/praccing", dto);

            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<PraccingResponseDto>();
            if (created == null)
                return null;
            var createdEntity = _responseMapper.MapToEntity(created);
            return createdEntity;
        } 

        public async Task<PraccingImage> AddPraccingImage(PraccingImage image)
        {
            var dto = _mapper.MapToDto(image);
            var response = await _http.PostAsJsonAsync("api/praccing/images", dto);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<PraccingImageDto>();
            if (created == null)
                return null;
            var createdEntity = _mapper.MapToEntity(created);
            return createdEntity;
        }

        public async Task<int> DeletePraccingImage(int id)
        {
            var result = await _http.DeleteAsync($"api/praccing/images/{id}");
            result.EnsureSuccessStatusCode();
            return 0;
        }

        public async Task<PraccingImage> AddResponseImage(PraccingImage image)
        {
            var dto = _mapper.MapToDto(image);
            var response = await _http.PostAsJsonAsync("api/praccing/responses/images", dto);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<PraccingImageDto>();
            if (created == null)
                return null;
            var createdEntity = _mapper.MapToEntity(created);
            return createdEntity;
        }

        public async Task<int> DeletePraccingResponseImage(int id)
        {
            var result = await _http.DeleteAsync($"api/praccing/responses/images/{id}");
            result.EnsureSuccessStatusCode();
            return 0;
        }

        public async Task<List<Survey>> GetPraccingSurveys()
        {
            var result = await _http.GetFromJsonAsync<List<SurveyDto>>($"api/praccing/surveys");

            if (result == null)
                return new List<Survey>();

            return result.Select(x => new Survey(x.SurveyCode) { SID = x.SID }).ToList();
        }
    }
}

