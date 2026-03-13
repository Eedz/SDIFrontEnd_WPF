using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ITCLib;
using ITC_Contracts;

namespace SDIFrontEnd_WPF
{
    public class ApiWordingService : ApiServiceBase, IApiWordingService
    {
        public ApiWordingService(HttpClient http) : base(http)
        {

        }

        Task<List<Wording>> IApiWordingService.GetAllPreP()
        {
            throw new NotImplementedException();
        }

        Task<List<Wording>> IApiWordingService.GetAllPreI()
        {
            throw new NotImplementedException();
        }

        Task<List<Wording>> IApiWordingService.GetAllPreA()
        {
            throw new NotImplementedException();
        }

        Task<List<Wording>> IApiWordingService.GetAllLitQ()
        {
            throw new NotImplementedException();
        }

        Task<List<Wording>> IApiWordingService.GetAllPstI()
        {
            throw new NotImplementedException();
        }

        Task<List<Wording>> IApiWordingService.GetAllPstP()
        {
            throw new NotImplementedException();
        }

        Task<List<ResponseSet>> IApiWordingService.GetAllRespOptions()
        {
            throw new NotImplementedException();
        }

        Task<List<ResponseSet>> IApiWordingService.GetAllNonResponses()
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
    }
}
