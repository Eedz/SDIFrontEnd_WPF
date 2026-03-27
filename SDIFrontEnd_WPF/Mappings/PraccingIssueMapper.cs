using DocumentFormat.OpenXml.Office2010.Excel;
using ITC_Contracts;
using ITCLib;

namespace SDIFrontEnd_WPF.Mappings
{
    public class PraccingIssueMapper : IMapper<PraccingIssue, PraccingIssueDto>
    {
        private readonly PraccingResponseMapper _responseMapper;

        public PraccingIssueMapper (PraccingResponseMapper responseMapper)
        {
            _responseMapper = responseMapper;

        }
        public PraccingIssueDto MapToDto(PraccingIssue entity)
        {
            if (entity == null) return null;

            return new PraccingIssueDto
            {
                ID = entity.ID,
                IssueNo = entity.IssueNo,

                Survey = new SurveyDto() { SID = entity.Survey?.SID ?? 0 },

                VarNames = entity.VarNames,
                Description = entity.Description,

                IssueDate = entity.IssueDate,

                IssueFrom =new PersonDto() { Name = entity.IssueFrom.Name, ID = entity.IssueFrom.ID },
                IssueTo = new PersonDto() { ID = entity.IssueTo.ID, Name = entity.IssueTo.Name },

                Resolved = entity.Resolved,
                ResolvedDate = entity.ResolvedDate,
                ResolvedBy = new PersonDto() { ID = entity.ResolvedBy.ID, Name = entity.ResolvedBy.Name },

                LastUpdate = entity.LastUpdate,

                Language = entity.Language,
                Fixed = entity.Fixed,

                Category = new PraccingCategoryDto() { ID = entity.Category.ID, Category = entity.Category.Category },

                EnteredBy = new PersonDto() { ID = entity.EnteredBy.ID, Name = entity.EnteredBy.Name },
                EnteredOn = entity.EnteredOn,

                PinNo = entity.PinNo,

                Images = entity.Images?.Select(x => new PraccingImageDto
                {
                    ID = x.ID,
                    PraccID = x.PraccID,
                    Path = x.Path,
                    FilePath = x.FilePath,
                }).ToList(),

                Responses = entity.Responses?.Select(x => _responseMapper.MapToDto(x)).ToList()
            };
        }

        public PraccingIssue MapToEntity(PraccingIssueDto dto)
        {
            if (dto == null) return null;

            return new PraccingIssue
            {
                ID = dto.ID,
                IssueNo = dto.IssueNo,

                Survey = new Survey { SID = dto.Survey.SID },

                VarNames = dto.VarNames,
                Description = dto.Description,

                IssueDate = dto.IssueDate,

                IssueFrom = new Person { ID = dto.IssueFrom.ID, Name = dto.IssueFrom.Name },
                IssueTo = new Person { ID = dto.IssueTo.ID, Name = dto.IssueTo.Name },

                Resolved = dto.Resolved,
                ResolvedDate = dto.ResolvedDate,
                ResolvedBy = new Person { ID = dto.ResolvedBy.ID, Name = dto.ResolvedBy.Name },

                Language = dto.Language,
                Fixed = dto.Fixed,

                Category = new PraccingCategory { ID = dto.Category.ID, Category = dto.Category.Category },

                EnteredBy = new Person { ID = dto.EnteredBy.ID, Name = dto.EnteredBy.Name },
                EnteredOn = dto.EnteredOn,

                PinNo = dto.PinNo,

                Images = dto.Images?.Select(x => new PraccingImage
                {
                    ID = x.ID,
                    PraccID = x.PraccID,
                    Path = x.Path,
                    FilePath = x.FilePath,
                }).ToList(),

                Responses = dto.Responses?.Select(x => _responseMapper.MapToEntity(x)).ToList()
            };
        }

        public PraccingImage MapToEntity(PraccingImageDto dto)
        {
            return new PraccingImage
                {
                ID = dto.ID,
                    PraccID = dto.PraccID,
                    Path = dto.Path,
                    FilePath = dto.FilePath,
                };
        }
        public PraccingImageDto MapToDto(PraccingImage dto)
        {
            return new PraccingImageDto
            {
                ID = dto.ID,
                PraccID = dto.PraccID,
                Path = dto.Path,
                FilePath = dto.FilePath,
            };
        }
    }
}
