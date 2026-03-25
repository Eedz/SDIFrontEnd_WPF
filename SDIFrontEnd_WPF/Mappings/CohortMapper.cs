using ITC_Contracts;
using ITCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF.Mappings
{
    public class CohortMapper : IMapper<SurveyCohort, SurveyCohortDto>
    {
        public SurveyCohort MapToEntity(SurveyCohortDto dto)
        {
            return new SurveyCohort
            {
                ID = dto.ID,
                Cohort = dto.Cohort,
                Code = dto.Code,
                WebName = dto.WebName
            };
        }

        public SurveyCohortDto MapToDto(SurveyCohort entity)
        {
            return new SurveyCohortDto
            {
                ID = entity.ID,
                Cohort = entity.Cohort,
                Code = entity.Code,
                WebName = entity.WebName
            };
        }

        public void MapToExisting(SurveyCohortDto dto, SurveyCohort entity)
        {
        }
    }
}
