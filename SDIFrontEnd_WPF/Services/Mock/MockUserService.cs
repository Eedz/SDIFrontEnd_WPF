using ITCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using ITC_Contracts;
namespace SDIFrontEnd_WPF
{
    public class MockUserService : IApiUserService
    {
        public MockUserService()
        {
        }

        public async Task<UserPrefs> GetUser(string username)
        {
            throw new NotImplementedException();
        }
    }
}
