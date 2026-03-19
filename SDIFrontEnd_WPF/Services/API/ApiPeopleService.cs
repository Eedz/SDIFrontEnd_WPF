
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
    public  class ApiPeopleService : ApiServiceBase, IApiPeopleService
    {
        public ApiPeopleService(HttpClient httpClient) : base(httpClient)
        {

            
        }

        public async Task<List<Person>> GetPeopleBasics()
        {
            try
            {
                var response = await this._http.GetFromJsonAsync<List<PersonDto>>($"api/people/basics");
                if (response == null)
                    return new List<Person>();

                var people = response.Select(dto => new Person
                {
                    ID = dto.ID,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Name = dto.Name,
                    Email = dto.Email,
                    Username = dto.Username,
                    OfficeNo = dto.OfficeNo,
                    WorkPhone = dto.WorkPhone,
                    HomePhone = dto.HomePhone,
                    Institution = dto.Institution,
                    Active = dto.Active,
                    PraccID = dto.PraccID,
                    Entry = dto.Entry,
                    PraccEntry = dto.PraccEntry,
                }).ToList();

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
