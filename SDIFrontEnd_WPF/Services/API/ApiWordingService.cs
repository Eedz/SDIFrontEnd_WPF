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

            var response = await _http.GetFromJsonAsync<List<ResponseSetDto>>($"api/wordings/{type}");
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



        

       
    }
}
