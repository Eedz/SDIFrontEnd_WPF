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
                Domain = new VarNameLabelDto() { LabelText = entity.DomainLabel.Label, ID = entity.DomainLabel.ID },
                Topic = new VarNameLabelDto() { LabelText = entity.TopicLabel.Label, ID = entity.TopicLabel.ID },
                Content = new VarNameLabelDto() { LabelText = entity.ContentLabel.Label, ID = entity.ContentLabel.ID },
                Product = new VarNameLabelDto() { LabelText = entity.ProductLabel.Label, ID = entity.ProductLabel.ID },
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
