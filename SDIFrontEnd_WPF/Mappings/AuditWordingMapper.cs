using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;
using ITC_Contracts;
namespace SDIFrontEnd_WPF.Mappings
{
    public class AuditWordingMapper : IMapper<AuditWording, AuditWordingDto>
    {
        public AuditWordingDto MapToDto(AuditWording entity)
        {
            throw new NotImplementedException();
        }

        public AuditWording MapToEntity(AuditWordingDto dto)
        {
            return new AuditWording
            {
                ID = dto.ID,
                WordingField = (WordingType)dto.Type,
                Wording = dto.WordingText
            };
        }


    }
}
