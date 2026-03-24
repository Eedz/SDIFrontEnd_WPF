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
    public class MockWordingService : IApiWordingService
    {
        public MockWordingService()
        {

        }

        private async Task<List<WordingDto>> GetWordingsByType(string type)
        {
            throw new NotImplementedException();
        }

        private async Task<List<ResponseSetDto>> GetResponseSetsByType(string type)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Wording>> GetAllPreP()
        {
            return new List<Wording>()
            {
                new Wording(){WordID=0, WordingText=string.Empty, Type=WordingType.PreP},
                new Wording(){WordID=1, WordingText="Ask all.", Type=WordingType.PreP},
            };
        }

        public async Task<List<Wording>> GetAllPreI()
        {
            return new List<Wording>()
            {
                new Wording(){WordID=0, WordingText=string.Empty, Type=WordingType.PreP},
                new Wording(){WordID=1, WordingText="Ask all.", Type=WordingType.PreP},
            };
        }

        public async Task<List<Wording>> GetAllPreA()
        {
            return new List<Wording>() { new Wording() { WordID = 0, WordingText = string.Empty }, new Wording() { WordID = 1, WordingText = "Ask all." }, };
        }

        public async Task<List<Wording>> GetAllLitQ()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Wording>> GetAllPstI()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Wording>> GetAllPstP()
        {
            throw new NotImplementedException();
        }

        public async Task<List<ResponseSet>> GetAllRespOptions()
        {
            throw new NotImplementedException();
        }

        public async Task<List<ResponseSet>> GetAllNonResponses()
        {
            throw new NotImplementedException();
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
