
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
        private readonly IMapper<Person, PersonDto> _personMapper;

        public ApiPeopleService(HttpClient httpClient, IMapperFactory mapperFactory) : base(httpClient)
        {
            _personMapper = mapperFactory.Get<Person, PersonDto>();

        }

        public async Task<List<Person>> GetPeopleBasics()
        {
            try
            {
                var response = await this._http.GetFromJsonAsync<List<PersonDto>>($"{_baseApi}/people/basics");
                if (response == null)
                    return new List<Person>();

                var people = response.Select(dto => _personMapper.MapToEntity(dto)).ToList();

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
