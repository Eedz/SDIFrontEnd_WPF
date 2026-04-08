using ITC_Contracts;
using ITCLib;
using SDIFrontEnd_WPF.Mappings;
using SDIFrontEnd_WPF.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    public class ApiWordingService : ApiServiceBase, IApiWordingService
    {
        private readonly IMapper<Wording, WordingDto> wordingMapper;
        private readonly IMapper<ResponseSet, ResponseSetDto> responseMapper;
        public ApiWordingService(HttpClient http, IMapperFactory mapperFactory) : base(http)
        {
            wordingMapper = mapperFactory.Get<Wording, WordingDto>(); ;
            responseMapper = mapperFactory.Get<ResponseSet, ResponseSetDto>(); ;
        }

        private async Task<List<WordingDto>> GetWordingsByType(string type)
        {
            switch (type.ToLower())
            {
                case "prep": 
                case "prei":
                case "prea":
                case "litq":
                case "psti":
                case "pstp":
                    break;
                default:
                    throw new ArgumentException($"Invalid wording type: {type}");
            }

            var response = await _http.GetFromJsonAsync<List<WordingDto>>($"api/wordings/{type}");
            return response.ToList();
        }

        private async Task<List<ResponseSetDto>> GetResponseSetsByType(string type)
        {
            switch (type.ToLower())
            {
                case "respoptions":
                case "nrcodes":
                    break;
                default:
                    throw new ArgumentException($"Invalid wording type: {type}");
            }

            var response = await _http.GetFromJsonAsync<List<ResponseSetDto>>($"api/responses/{type}");
            return response.ToList();
        }

        public async Task<List<Wording>> GetAllPreP()
        {
            var response = await GetWordingsByType("prep");

            var wordings = response.Select(dto => wordingMapper.MapToEntity(dto)).ToList();
            return wordings;
        }

        public async Task<List<Wording>> GetAllPreI()
        {
            var response = await GetWordingsByType("prei");

            var wordings = response.Select(dto => wordingMapper.MapToEntity(dto)).ToList();
            return wordings;
        }

        public async Task<List<Wording>> GetAllPreA()
        {
            var response = await GetWordingsByType("prea");

            var wordings = response.Select(dto => wordingMapper.MapToEntity(dto)).ToList();
            return wordings;
        }

        public async Task<List<Wording>> GetAllLitQ()
        {
            var response = await GetWordingsByType("litq");

            var wordings = response.Select(dto => wordingMapper.MapToEntity(dto)).ToList();
            return wordings;
        }

        public async Task<List<Wording>> GetAllPstI()
        {
            var response = await GetWordingsByType("psti");

            var wordings = response.Select(dto => wordingMapper.MapToEntity(dto)).ToList();
            return wordings;
        }

        public async Task<List<Wording>> GetAllPstP()
        {
            var response = await GetWordingsByType("pstp");

            var wordings = response.Select(dto => wordingMapper.MapToEntity(dto)).ToList();
            return wordings;
        }

        public async Task<List<ResponseSet>> GetAllRespOptions()
        {
            var response = await GetResponseSetsByType("respoptions");

            var wordings = response.Select(dto => responseMapper.MapToEntity(dto)).ToList();
            return wordings;
        }

        public async Task<List<ResponseSet>> GetAllNonResponses()
        {
            var response = await GetResponseSetsByType("nrcodes");

            var wordings = response.Select(dto => responseMapper.MapToEntity(dto)).ToList();
            return wordings;
        }

        public async Task<Wording> UpdateWording(Wording wording)
        {
            string type = wording.FieldType.ToLower();
            var dto = wordingMapper.MapToDto(wording);
            var response = await _http.PutAsJsonAsync<WordingDto>($"api/wordings/{type}/{wording.WordID}/", dto);
            response.EnsureSuccessStatusCode();

            var updatedDto = await response.Content.ReadFromJsonAsync<WordingDto>();
            return wordingMapper.MapToEntity(updatedDto);
        }

        public async Task<ResponseSet> UpdateResponseSet(ResponseSet responseSet)
        {
            string type = responseSet.FieldType.ToLower();
            var dto = responseMapper.MapToDto(responseSet);
            var response = await _http.PutAsJsonAsync<ResponseSetDto>($"api/responses/{type}/{responseSet.RespSetName}/", dto);
            response.EnsureSuccessStatusCode();
            var updatedDto = await response.Content.ReadFromJsonAsync<ResponseSetDto>();
            return responseMapper.MapToEntity(updatedDto);
        }

        public async Task<List<WordingUsage>> GetWordingUsages(Wording wording)
        {
            string type = wording.FieldType.ToLower();
            var response = await _http.GetFromJsonAsync<List<WordingUsageDto>>($"api/wordings/{type}/{wording.WordID}/usages");
            var wordings = response.Select(dto => new WordingUsage()
            {
                VarName = dto.VarName,
                VarLabel = dto.VarLabel,
                Qnum = dto.Qnum,
                Locked = dto.Locked,
                WordID = dto.WordID,
                SurveyCode = dto.SurveyCode
            }).ToList();
                
            return wordings;
        }

        public async Task<List<ResponseUsage>> GetResponseUsages(ResponseSet responseset)
        {
            string type = responseset.FieldType.ToLower();
            var response = await _http.GetFromJsonAsync<List<ResponseUsageDto>>($"api/responses/{type}/{responseset.RespSetName}/usages");
            var wordings = response.Select(dto => new ResponseUsage()
            {
                VarName = dto.VarName,
                VarLabel = dto.VarLabel,
                Qnum = dto.Qnum,
                Locked = dto.Locked,
                RespName = dto.RespName,
                SurveyCode = dto.SurveyCode
            }).ToList();

            return wordings;
        }

        public async Task<Wording> CreateWording(Wording wording)
        {
            string type = wording.FieldType.ToLower();
            var dto = wordingMapper.MapToDto(wording);
            var response = await _http.PostAsJsonAsync($"api/wordings/{wording.FieldType}", dto);
            if (response.IsSuccessStatusCode)
            {
                var createdDto = await response.Content.ReadFromJsonAsync<WordingDto>();
                return wordingMapper.MapToEntity(createdDto);
            }
            else
            {
                throw new Exception($"Failed to create wording: {response.ReasonPhrase}");
            }
        }

        public async Task<ResponseSet> CreateResponseSet(ResponseSet responseSet)
        {
            string type = responseSet.FieldType.ToLower();
            var dto = responseMapper.MapToDto(responseSet);
            var response = await _http.PostAsJsonAsync($"api/responses/{responseSet.FieldType}", dto);
            if (response.IsSuccessStatusCode)
            {
                var createdDto = await response.Content.ReadFromJsonAsync<ResponseSetDto>();
                return responseMapper.MapToEntity(createdDto);
            }
            else
            {
                throw new Exception($"Failed to create response set: {response.ReasonPhrase}");
            }
        }

        public async Task<bool> DeleteWording(Wording wording)
        {
            string type = wording.FieldType.ToLower();
            var response = await _http.DeleteAsync($"api/wordings/{type}/{wording.WordID}/");
            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<bool> DeleteResponseSet(ResponseSet set)
        {
            string type = set.FieldType.ToLower();
            var response = await _http.DeleteAsync($"api/responses/{type}/{set.RespSetName}/");
            response.EnsureSuccessStatusCode();
            return true;
        }



        

       
    }
}
