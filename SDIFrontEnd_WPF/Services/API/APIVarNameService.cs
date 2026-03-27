using ITC_Contracts;
using ITCLib;
using SDIFrontEnd_WPF.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    /// <summary>
    /// DTO -> Entity mapping and API calls for VariableName related operations.
    /// </summary>
    public class ApiVarNameService : ApiServiceBase, IApiVarNameService
    {
        VarNameMapper _varnameMapper;

        public ApiVarNameService(HttpClient http, VarNameMapper varnameMapper) : base(http)
        {
            _varnameMapper = varnameMapper;
        }

        public async Task<int> InsertVariable(VariableName variable)
        {
            
            var dto = _varnameMapper.MapToDto(variable);
            var result = await _http.PostAsJsonAsync("api/varnames", dto);
            return result.IsSuccessStatusCode ? 1 : 0;
        }

        public async Task<List<VariableName>> GetAllVarNames()
        {
            var list = await _http.GetFromJsonAsync<List<VariableNameDto>>("api/varnames/all");
            return list.Select(_varnameMapper.MapToEntity).ToList();
        }

        public async Task<List<VariableName>> GetAllVarNamesByRefList(List<string> varlist)
        {
            List<VariableName> list = new List<VariableName>();

            foreach (string varname in varlist)
            {
                var varnames = await _http.GetFromJsonAsync<VariableNameDto>($"api/varnames/{varname}");
                list.Add(_varnameMapper.MapToEntity(varnames));
            }
            return list;
        }

        public async Task<VariableName> GetVariableInfo(string varname)
        {
            var dto = await _http.GetFromJsonAsync<VariableNameDto>($"api/varnames/{varname}");
            return dto == null ? new VariableName() : _varnameMapper.MapToEntity(dto);
        }

        public async Task<List<VariableName>> GetVariableInfoByRef(string refvarname)
        {
            throw new NotImplementedException();
        }

        public async Task<List<VariableName>> SearchVarNames(string search, int take = 50)
        {
            var dto = await _http.GetFromJsonAsync<List<VariableNameDto>>($"api/varnames?search={Uri.EscapeDataString(search)}&take={take}");
            return dto == null ? new List<VariableName>() : dto.Select(_varnameMapper.MapToEntity).ToList();
        }

        public async Task<bool> UpdateVariable(VariableName variable)
        {
            var dto = _varnameMapper.MapToDto(variable);
            var result = await _http.PutAsJsonAsync($"api/varnames/{variable.ID}", dto);
            return result.IsSuccessStatusCode;
        }

        public async Task<bool> VarNameInUse(string varname)
        {
            var result = await _http.GetAsync($"api/varnames/exists?varName={varname}");
            if (result.IsSuccessStatusCode)
            {
                var inUse = await result.Content.ReadFromJsonAsync<bool>();
                return inUse;
            }
            else
            {
                return false;
            }
        }



        public async Task<bool> DeleteVariable(string varname)
        {
            var result = await _http.DeleteAsync($"api/varnames/{varname}");
            return result.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteVariable(int id)
        {
            var dto = await _http.DeleteAsync($"api/varnames/{id}");
            return dto.IsSuccessStatusCode;
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
            var dto = await _http.GetFromJsonAsync<List<VariableNameSurveys>>($"api/varnames/usage?search={Uri.EscapeDataString(searchTerm)}&take={take}");
            return dto ?? new List<VariableNameSurveys>();
        }

       
    }
}
