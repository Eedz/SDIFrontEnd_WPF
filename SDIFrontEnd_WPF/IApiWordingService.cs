using ITC_Contracts;
using ITCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
namespace SDIFrontEnd_WPF
{
    public interface IApiWordingService
    {
        Task<List<Wording>> GetAllPreP();
        Task<List<Wording>> GetAllPreI();
        Task<List<Wording>> GetAllPreA();
        Task<List<Wording>> GetAllLitQ();
        Task<List<Wording>> GetAllPstI();
        Task<List<Wording>> GetAllPstP();
        Task<List<ResponseSet>> GetAllRespOptions();
        Task<List<ResponseSet>> GetAllNonResponses();
        Task<Wording> UpdateWording(Wording wording);
        Task<ResponseSet> UpdateResponseSet(ResponseSet responseSet);

        Task<List<WordingUsage>> GetWordingUsages(Wording wording);
        Task<List<ResponseUsage>> GetResponseUsages(ResponseSet response);

        Task<Wording> CreateWording(Wording wording);
        Task<ResponseSet> CreateResponseSet(ResponseSet responseSet);

        Task<int> DeleteWording(Wording wording);
        Task<int> DeleteResponseSet(ResponseSet set);
    }
}
