using ITC_Contracts;
using ITCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF.Mappings
{
    public class PersonMapper : IMapper<Person, PersonDto>
    {
        public PersonDto MapToDto(Person entity)
        {
            throw new NotImplementedException();
        }

        public Person MapToEntity(PersonDto dto)
        {
            return new Person
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
            };
        }
    }
}
