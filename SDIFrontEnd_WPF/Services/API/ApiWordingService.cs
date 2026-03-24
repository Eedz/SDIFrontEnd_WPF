using ITC_Contracts;
using ITCLib;
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
        public ApiWordingService(HttpClient http) : base(http)
        {

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

            var response = await _http.GetFromJsonAsync<List<ResponseSetDto>>($"api/wordings/{type}");
            return response.ToList();
        }

        public async Task<List<Wording>> GetAllPreP()
        {
            var response = await GetWordingsByType("prep");

            var wordings = response.Select(dto => MapToEntity(dto)).ToList();
            return wordings;
        }

        public async Task<List<Wording>> GetAllPreI()
        {
            var response = await GetWordingsByType("prei");

            var wordings = response.Select(dto => MapToEntity(dto)).ToList();
            return wordings;
        }

        public async Task<List<Wording>> GetAllPreA()
        {
            var response = await GetWordingsByType("prea");

            var wordings = response.Select(dto => MapToEntity(dto)).ToList();
            return wordings;
        }

        public async Task<List<Wording>> GetAllLitQ()
        {
            var response = await GetWordingsByType("litq");

            var wordings = response.Select(dto => MapToEntity(dto)).ToList();
            return wordings;
        }

        public async Task<List<Wording>> GetAllPstI()
        {
            var response = await GetWordingsByType("psti");

            var wordings = response.Select(dto => MapToEntity(dto)).ToList();
            return wordings;
        }

        public async Task<List<Wording>> GetAllPstP()
        {
            var response = await GetWordingsByType("pstp");

            var wordings = response.Select(dto => MapToEntity(dto)).ToList();
            return wordings;
        }

        public async Task<List<ResponseSet>> GetAllRespOptions()
        {
            var response = await GetResponseSetsByType("respoptions");

            var wordings = response.Select(dto => MapToEntity(dto)).ToList();
            return wordings;
        }

        public async Task<List<ResponseSet>> GetAllNonResponses()
        {
            var response = await GetResponseSetsByType("nrcodes");

            var wordings = response.Select(dto => MapToEntity(dto)).ToList();
            return wordings;
        }

        Task<Wording> IApiWordingService.UpdateWording(Wording wording)
        {
            throw new NotImplementedException();
        }

        Task<ResponseSet> IApiWordingService.UpdateResponseSet(ResponseSet responseSet)
        {
            throw new NotImplementedException();
        }

        Task<List<WordingUsage>> IApiWordingService.GetWordingUsages(Wording wording)
        {
            throw new NotImplementedException();
        }

        Task<List<ResponseUsage>> IApiWordingService.GetResponseUsages(ResponseSet response)
        {
            throw new NotImplementedException();
        }

        Task<Wording> IApiWordingService.CreateWording(Wording wording)
        {
            throw new NotImplementedException();
        }

        Task<ResponseSet> IApiWordingService.CreateResponseSet(ResponseSet responseSet)
        {
            throw new NotImplementedException();
        }

        Task<int> IApiWordingService.DeleteWording(Wording wording)
        {
            throw new NotImplementedException();
        }

        Task<int> IApiWordingService.DeleteResponseSet(ResponseSet set)
        {
            throw new NotImplementedException();
        }

        private Wording MapToEntity(WordingDto dto)
        {
            return new Wording
            {
                WordID = dto.ID,
                Type = (WordingType)dto.Type,
                WordingText = dto.WordingText
            };
        }

        private ResponseSet MapToEntity(ResponseSetDto dto)
        {
            return new ResponseSet
            {
                RespSetName = dto.RespSetName,
                Type = (ResponseType)dto.Type,
                RespList = dto.RespList
            };
        }

        private WordingDto MapToDto(Wording entity)
        {
            return new WordingDto
            {
                ID = entity.WordID,
                Type = (int)entity.Type,
                WordingText = entity.WordingText
            };
        }
    }
}
