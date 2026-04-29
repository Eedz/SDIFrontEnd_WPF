using ITC_Contracts;
using ITCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF.Mappings
{
    public class VarNameMapper : IMapper<VariableName, VariableNameDto>
    {
        public VariableNameDto MapToDto(VariableName entity)
        {
            return new VariableNameDto
            {
                ID = entity.ID,
                VarName = entity.VarName,
                VarLabel = entity.VarLabel,
                Domain = new VarNameLabelDto() { LabelText = entity.Domain.LabelText, ID = entity.Domain.ID },
                Topic = new VarNameLabelDto() { LabelText = entity.Topic.LabelText, ID = entity.Topic.ID },
                Content = new VarNameLabelDto() { LabelText = entity.Content.LabelText, ID = entity.Content.ID },
                Product = new VarNameLabelDto() { LabelText = entity.Product.LabelText, ID = entity.Product.ID },
            };
        }

        public VariableName MapToEntity(VariableNameDto dto)
        {
            return new VariableName
            {
                ID = dto.ID,
                VarName = dto.VarName,
                VarLabel = dto.VarLabel,
                DomainLabel = new VarNameLabel() { Label = dto.Domain.LabelText, ID = dto.Domain.ID },
                TopicLabel = new VarNameLabel() { Label = dto.Topic.LabelText, ID = dto.Topic.ID },
                ContentLabel = new VarNameLabel() { Label = dto.Content.LabelText, ID = dto.Content.ID },
                ProductLabel = new VarNameLabel() { Label = dto.Product.LabelText, ID = dto.Product.ID },
                Domain = new DomainLabel() { LabelText = dto.Domain.LabelText, ID = dto.Domain.ID },
                Topic = new TopicLabel() { LabelText = dto.Topic.LabelText, ID = dto.Topic.ID },
                Content = new ContentLabel() { LabelText = dto.Content.LabelText, ID = dto.Content.ID },
                Product = new ProductLabel() { LabelText = dto.Product.LabelText, ID = dto.Product.ID },
            };
        }

    }
}
