using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using ITCLib;

namespace SDIFrontEnd_WPF
{
    public class MockPraccingService : IApiPraccingService
    {
        public MockPraccingService()
        {
        }

        public Task<int> DeletePraccingIssue(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<PraccingCategory>> GetPraccingCategories()
        {
            throw new NotImplementedException();
        }

        public Task<List<PraccingIssue>> GetPraccingIssues(int surveyid)
        {
            throw new NotImplementedException();
        }

        public Task<PraccingIssue> AddPraccingIssue(PraccingIssue issue)
        {
            throw new NotImplementedException();
        }

        public Task<PraccingIssue> UpdatePraccingIssue(PraccingIssue issue)
        {
            throw new NotImplementedException();
        }

        public Task<PraccingResponse> AddPraccingResponse(PraccingResponse response)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeletePraccingResponse(int id)
        {
            throw new NotImplementedException();
        }

        public Task<PraccingResponse> UpdatePraccingResponse(PraccingResponse response)
        {
            throw new NotImplementedException();
        } 

        public Task<PraccingImage> AddPraccingImage(PraccingImage image)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeletePraccingImage(int id)
        {
            throw new NotImplementedException();
        }

        public Task<PraccingImage> AddResponseImage(PraccingImage image)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeletePraccingResponseImage(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Survey>> GetPraccingSurveys()
        {
            throw new NotImplementedException();
        }
    }
}

