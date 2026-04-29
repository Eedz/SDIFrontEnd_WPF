
using ITC_Contracts;
using ITCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    public  class MockPeopleService : IApiPeopleService
    {
        public MockPeopleService()
        {

            
        }

        public async Task<List<Person>> GetPeopleBasics()
        {
            return await Task.FromResult(new List<Person>
            {
                new Person { ID = 1, Name = "Alice" },
                new Person { ID = 2, Name = "Bob" },
                new Person { ID = 3, Name = "Charlie" }
            });
        }
    }
}
