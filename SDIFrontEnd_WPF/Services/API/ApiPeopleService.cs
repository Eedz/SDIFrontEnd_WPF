
using ITC_Contracts;
using ITCLib;
using SDIFrontEnd_WPF.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    public  class ApiPeopleService : ApiServiceBase, IApiPeopleService
    {
        PersonMapper _mapper;

        public ApiPeopleService(HttpClient httpClient, PersonMapper mapper) : base(httpClient)
        {
            _mapper = mapper;
            
        }

        public async Task<List<Person>> GetPeopleBasics()
        {
            try
            {
                var response = await this._http.GetFromJsonAsync<List<PersonDto>>($"api/people/basics");
                if (response == null)
                    return new List<Person>();

                var people = response.Select(dto => _mapper.MapToEntity(dto)).ToList();

                return people;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new ApplicationException("Error fetching people basics", ex);
            }
        }
    }
}
