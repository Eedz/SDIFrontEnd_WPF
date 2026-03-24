using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using ITC_Contracts;
using ITCLib;
using System.Collections.ObjectModel;
namespace SDIFrontEnd_WPF
{
    public class MockReferenceDataService : IApiReferenceDataService
    {
       

        public MockReferenceDataService() 
        {

        }

        public async Task<ReferenceDataStore> GetReferenceData()
        {
            
            
            return new ReferenceDataStore();
        }

        
    }
}
