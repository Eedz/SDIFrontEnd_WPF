using ITC_Contracts;
using ITC_Services;
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
    public class ApiVarNameService : ApiServiceBase, IApiVarNameService
    {
        public ApiVarNameService(HttpClient http) : base(http)
        {
        }

        public int InsertVariable(VariableName dto)
        {
            return 0;
            //var entity = MapToEntity(dto);
            //return _service.InsertVariable(entity);

        }

        public async Task<List<VariableName>> GetAllVarNames()
        {
            var list = await _http.GetFromJsonAsync<List<VariableNameDto>>("api/varnames");
            return list.Select(MapToEntity).ToList();
        }

        public async Task<List<VariableName>> GetAllVarNamesByRefList(List<string> varlist)
        {
            List<VariableName> list = new List<VariableName>();

            foreach (string varname in varlist)
            {
                var varnames = await _http.GetFromJsonAsync<VariableNameDto>($"api/varnames/{varname}");
                list.Add(MapToEntity(varnames));
            }
            return list;
        }

        public async Task<VariableName> GetVariableInfo(string varname)
        {
            var dto = await _http.GetFromJsonAsync<VariableNameDto>($"api/varnames/{varname}");
            return dto == null ? new VariableName() : MapToEntity(dto);
        }

        public async Task<List<VariableName>> GetVariableInfoByRef(string refvarname)
        {
            //var list = _http.GetFromJsonAsAsyncEnumerable<VariableNameDto>($"api/varnames/{refvarname}");
            return null;
        }

        public async Task<List<VariableName>> SearchVarNames(string search, int take = 50)
        {
            var dto = await _http.GetFromJsonAsync<List<VariableNameDto>>($"api/varnames?search={Uri.EscapeDataString(search)}&take={take}");
            return dto == null ? new List<VariableName>() : dto.Select(MapToEntity).ToList();
        }

        private VariableNameDto MapToDto(VariableName entity)
        {
            return new VariableNameDto
            {
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
                VarName = dto.VarName,
                VarLabel = dto.VarLabel,
                Domain = new DomainLabel() { LabelText = dto.Domain.LabelText, ID = dto.Domain.ID },
                Topic = new TopicLabel() { LabelText = dto.Topic.LabelText, ID = dto.Topic.ID },
                Content = new ContentLabel() { LabelText = dto.Content.LabelText, ID = dto.Content.ID },
                Product = new ProductLabel() { LabelText = dto.Product.LabelText, ID = dto.Product.ID },
            };
        }
    }
}
