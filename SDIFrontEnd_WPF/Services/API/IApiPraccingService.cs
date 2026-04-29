using ITCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    public interface IApiPraccingService
    {
        Task<int> DeletePraccingIssue(int id);
        Task<List<PraccingCategory>> GetPraccingCategories();
        Task<List<PraccingIssue>> GetPraccingIssues(int surveyid);  
        Task<PraccingIssue> AddPraccingIssue(PraccingIssue issue);
        Task<PraccingIssue> UpdatePraccingIssue(PraccingIssue issue);

        Task<PraccingResponse> AddPraccingResponse(PraccingResponse response);
        Task<int> DeletePraccingResponse(int id);   

        Task<PraccingResponse> UpdatePraccingResponse(PraccingResponse response);

        Task<PraccingImage> AddPraccingImage(PraccingImage image);
        Task<int> DeletePraccingImage(int id);  

        Task<PraccingImage> AddResponseImage(PraccingImage image);
        Task<int> DeletePraccingResponseImage(int id);

        Task<List<Survey>> GetPraccingSurveys();
    }
}
