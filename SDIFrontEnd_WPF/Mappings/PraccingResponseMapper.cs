using ITC_Contracts;
using ITCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF.Mappings
{
    public class PraccingResponseMapper : IMapper<PraccingResponse, PraccingResponseDto>
    {
        public PraccingResponse MapToEntity(PraccingResponseDto dto)
        {
            if (dto == null)
                return null;

            return new PraccingResponse()
            {
                ID = dto.ID,
                IssueID = dto.IssueID,
                ResponseDate  = dto.ResponseDate,
                Response = dto.Response,
                ResponseFrom = new Person() { ID =  dto.ResponseFrom.ID , Name = dto.ResponseFrom.Name},
                ResponseTo = new Person() { ID = dto.ResponseTo.ID, Name = dto.ResponseTo.Name },
                PinNo = dto.PinNo,
                Images = dto.Images.Select(x=>new PraccingImage()
                {
                    ID = x.ID,
                    PraccID = x.PraccID,
                    Path = x.Path,
                    FilePath = x.FilePath,

                }).ToList(),

            };

        }

        public PraccingResponseDto MapToDto (PraccingResponse entity)
        {
            if (entity == null) return null;

            return new PraccingResponseDto()
            {
                ID = entity.ID,
                IssueID = entity.IssueID,
                ResponseDate = entity.ResponseDate,
                Response = entity.Response,
                ResponseFrom = new PersonDto() { ID = entity.ResponseFrom.ID, Name = entity.ResponseFrom.Name },
                ResponseTo = new PersonDto() { ID = entity.ResponseTo.ID, Name = entity.ResponseTo.Name },
                PinNo = entity.PinNo,
                Images = entity.Images.Select(x => new PraccingImageDto()
                {
                    ID = x.ID,
                    PraccID = x.PraccID,
                    Path = x.Path,
                    FilePath = x.FilePath,

                }).ToList(),
            };
        }
    }
}
