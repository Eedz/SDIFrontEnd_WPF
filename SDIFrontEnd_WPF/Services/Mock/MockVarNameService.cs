using ITC_Contracts;
using ITCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace SDIFrontEnd_WPF
{
    /// <summary>
    /// DTO -> Entity mapping and API calls for VariableName related operations.
    /// </summary>
    public class MockVarNameService : IApiVarNameService
    {
        public MockVarNameService()
        {
        }

        public async Task<int> InsertVariable(VariableName variable)
        {

            throw new NotImplementedException();
        }

        public async Task<List<VariableName>> GetAllVarNames()
        {
            throw new NotImplementedException();
        }

        public async Task<List<VariableName>> GetAllVarNamesByRefList(List<string> varlist)
        {
            throw new NotImplementedException();
        }

        public async Task<VariableName> GetVariableInfo(string varname)
        {
            throw new NotImplementedException();
        }

        public async Task<List<VariableName>> GetVariableInfoByRef(string refvarname)
        {
            throw new NotImplementedException();
        }

        public async Task<List<VariableName>> SearchVarNames(string search, int take = 50)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateVariable(VariableName variable)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> VarNameInUse(string varname)
        {
            throw new NotImplementedException();
        }



        public async Task<bool> DeleteVariable(string varname)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteVariable(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteVariablePrefix(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<int> InsertVariablePrefix(VariablePrefix prefix)
        {
            throw new NotImplementedException();
        }

        public async Task<List<VariablePrefix>> GetVariablePrefixes()
        {
            throw new NotImplementedException();
        }

        public async Task<int> UpdateVariablePrefix(VariablePrefix prefix)
        {
            throw new NotImplementedException();
        }

        public async Task<List<VariableNameSurveys>> SearchVarNameUsage(string searchTerm, int take)
        {
            throw new NotImplementedException();
        }

        private VariableNameDto MapToDto(VariableName entity)
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

        private VariableName MapToEntity(VariableNameDto dto)
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
