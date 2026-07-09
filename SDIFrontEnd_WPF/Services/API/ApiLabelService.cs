using ITCLib;
using SDIFrontEnd_WPF.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ITC_Contracts;
using System.Net.Http.Json;
namespace SDIFrontEnd_WPF
{
    public class ApiLabelService : ApiServiceBase, IApiLabelService
    {
        private readonly IMapperFactory _mapperFactory;

        public ApiLabelService(HttpClient httpClient, IMapperFactory mapperFactory) : base(httpClient)
        {
            _mapperFactory = mapperFactory;
        }

        public async Task<List<VariableName>> GetVarNamesByLabel(string type, VarNameLabel label)
        {
            var dtos = await _http.GetFromJsonAsync<List<VariableNameDto>>($"{_baseApi}/labels/{type}/{label.ID}/varnames");

            return dtos.Select(dto => new VariableName()
            {
                VarName = dto.VarName,
                VarLabel = dto.VarLabel
            }).ToList();

        }
    }
}
