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
        private readonly IMapper<VariableName, VariableNameDto> _varnameMapper;

        public ApiVarNameService(HttpClient http, IMapperFactory mapperFactory) : base(http)
        {
            _varnameMapper = mapperFactory.Get<VariableName, VariableNameDto>();
        }

        public async Task<int> InsertVariable(VariableName variable)
        {

            var dto = _varnameMapper.MapToDto(variable);
            var result = await _http.PostAsJsonAsync($"{_baseApi}/varnames", dto);
            return result.IsSuccessStatusCode ? 1 : 0;
        }

        public async Task<List<VariableName>> GetAllVarNames()
        {
            var list = await _http.GetFromJsonAsync<List<VariableNameDto>>($"{_baseApi}/varnames/all");
            return list.Select(_varnameMapper.MapToEntity).ToList();
        }

        public async Task<List<VariableName>> GetAllVarNamesByRefList(List<string> varlist)
        {
            List<VariableName> list = new List<VariableName>();

            foreach (string varname in varlist)
            {
                var varnames = await _http.GetFromJsonAsync<VariableNameDto>($"{_baseApi}/varnames/{varname}");
                list.Add(_varnameMapper.MapToEntity(varnames));
            }
            return list;
        }

        public async Task<VariableName> GetVariableInfo(string varname)
        {
            var dto = await _http.GetFromJsonAsync<VariableNameDto>($"{_baseApi}/varnames/{varname}");
            return dto == null ? new VariableName() : _varnameMapper.MapToEntity(dto);
        }

        public async Task<List<VariableName>> GetVariableInfoByRef(string refvarname)
        {
            throw new NotImplementedException();
        }

        public async Task<List<VariableName>> SearchVarNames(string search, int take = 50)
        {
            var dto = await _http.GetFromJsonAsync<List<VariableNameDto>>($"{_baseApi}/varnames?search={Uri.EscapeDataString(search)}&take={take}");
            return dto == null ? new List<VariableName>() : dto.Select(_varnameMapper.MapToEntity).ToList();
        }

        public async Task<bool> UpdateVariable(VariableName variable)
        {
            var dto = _varnameMapper.MapToDto(variable);
            var result = await _http.PutAsJsonAsync($"{_baseApi}/varnames/{variable.ID}", dto);
            return result.IsSuccessStatusCode;
        }

        public async Task<bool> VarNameInUse(string varname)
        {
            var result = await _http.GetAsync($"{_baseApi}/varnames/exists?varName={varname}");
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
            var result = await _http.DeleteAsync($"{_baseApi}/varnames/{varname}");
            return result.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteVariable(int id)
        {
            var dto = await _http.DeleteAsync($"{_baseApi}/varnames/{id}");
            return dto.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteVariablePrefix(int id)
        {
            var response = await _http.DeleteAsync($"{_baseApi}/varnames/prefixes/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<int> InsertVariablePrefix(VariablePrefix prefix)
        {
            var dto = new VariablePrefixDto()
            {
                ID = prefix.ID,
                Prefix = prefix.Prefix,
                PrefixName = prefix.PrefixName,
                ProductType = prefix.ProductType,
                RelatedPrefixes = prefix.RelatedPrefixes,
                Description = prefix.Description,
                Comments = prefix.Comments,
                Inactive = prefix.Inactive
            };

            var response = await _http.PostAsJsonAsync<VariablePrefixDto>($"{_baseApi}/varnames/prefixes", dto);
            response.EnsureSuccessStatusCode();
            var createdDto = await response.Content.ReadFromJsonAsync<VariablePrefixDto>();
            return 0;
        }

        public async Task<List<VariablePrefix>> GetVariablePrefixes()
        {
            var response = await _http.GetFromJsonAsync<List<VariablePrefixDto>>($"{_baseApi}/varnames/prefixes");
            var prefixes = response.Select(x => new VariablePrefix()
            {
                ID = x.ID,
                Prefix = x.Prefix,
                PrefixName = x.PrefixName,
                ProductType = x.ProductType,
                RelatedPrefixes = x.RelatedPrefixes,
                Description = x.Description,
                Comments = x.Comments,
                Inactive = x.Inactive,
                Ranges = x.Ranges.Select(r => new VariableRange()
                {
                    ID = r.ID,
                    PrefixID = r.PrefixID,
                    Lower = r.Lower,
                    Upper = r.Upper,
                    Description = r.Description
                }).ToList()
            }).ToList();
            return prefixes;
        }

        public async Task<int> UpdateVariablePrefix(VariablePrefix prefix)
        {
            var dto = new VariablePrefixDto()
            {
                ID = prefix.ID,
                Prefix = prefix.Prefix,
                PrefixName = prefix.PrefixName,
                ProductType = prefix.ProductType,
                RelatedPrefixes = prefix.RelatedPrefixes,
                Description = prefix.Description,
                Comments = prefix.Comments,
                Inactive = prefix.Inactive,
                Ranges = prefix.Ranges.Select(x=>new VariableRangeDto() 
                { 
                    PrefixID = x.ID, 
                    Lower = x.Lower, 
                    Upper = x.Upper, 
                    Description = x.Description }).ToList()
            };

            var response = await _http.PutAsJsonAsync<VariablePrefixDto>($"{_baseApi}/varnames/prefixes/{prefix.ID}", dto);
            response.EnsureSuccessStatusCode();
            var updatedDto = await response.Content.ReadFromJsonAsync<VariablePrefixDto>();
            return 0;
        }

        public async Task<List<VariableNameSurveys>> SearchVarNameUsage(string searchTerm, int take)
        {
            var dto = await _http.GetFromJsonAsync<List<VarNameUsageDto>>($"{_baseApi}/varnames/usage?search={Uri.EscapeDataString(searchTerm)}&take={take}");
            var usages = new List<VariableNameSurveys>();
            dto.ForEach(x => usages.Add(new VariableNameSurveys()
            {
                ID = x.ID,
                VarName = x.VarName,
                RefVarName = Utilities.ChangeCC(x.VarName),
                VarLabel = x.VarLabel,
                DomainLabel = new VarNameLabel() { Label = x.Domain.LabelText, ID = x.Domain.ID },
                TopicLabel = new VarNameLabel() { Label = x.Topic.LabelText, ID = x.Topic.ID },
                ContentLabel = new VarNameLabel() { Label = x.Content.LabelText, ID = x.Content.ID },
                ProductLabel = new VarNameLabel() { Label = x.Product.LabelText, ID = x.Product.ID },
                Domain = new DomainLabel() { LabelText = x.Domain.LabelText, ID = x.Domain.ID },
                Topic = new TopicLabel() { LabelText = x.Topic.LabelText, ID = x.Topic.ID },
                Content = new ContentLabel() { LabelText = x.Content.LabelText, ID = x.Content.ID },
                Product = new ProductLabel() { LabelText = x.Product.LabelText, ID = x.Product.ID },
                SurveyList = x.SurveyList
            }));
            return usages;
        }

        public async Task<List<QuestionUsage>> GetVarNameQuestions(string varname)
        {
            var dto = await _http.GetFromJsonAsync<List<QuestionUsageDto>>($"{_baseApi}/varnames/usage/{varname}");
            var questions = dto.Select(x => new QuestionUsage
            {
                ID = x.ID,
                SurveyCode = x.SurveyCode,
                VarName = new VariableName(x.VarName.VarName)
                {
                    VarLabel = x.VarName.VarLabel,
                    Domain = new DomainLabel() { LabelText = x.VarName.Domain?.LabelText, ID = x.VarName.Domain?.ID ?? 0 },
                    Topic = new TopicLabel() { LabelText = x.VarName.Topic?.LabelText, ID = x.VarName.Topic?.ID ?? 0 },
                    Content = new ContentLabel() { LabelText = x.VarName.Content?.LabelText, ID = x.VarName.Content?.ID ?? 0 },
                    Product = new ProductLabel() { LabelText = x.VarName.Product?.LabelText, ID = x.VarName.Product?.ID ?? 0 },
                    DomainLabel = new VarNameLabel(x.VarName.Domain?.ID ?? 0, x.VarName.Domain?.LabelText),
                    TopicLabel = new VarNameLabel(x.VarName.Topic?.ID ?? 0, x.VarName.Topic?.LabelText),
                    ContentLabel = new VarNameLabel(x.VarName.Content?.ID ?? 0, x.VarName.Content?.LabelText),
                    ProductLabel = new VarNameLabel(x.VarName.Product?.ID ?? 0, x.VarName.Product?.LabelText)
                },
                Qnum = x.Qnum,
                AltQnum = x.AltQnum,
                PrePW = x.PrePW == null ? null : new Wording() { WordID = x.PrePW.ID, WordingText = x.PrePW.WordingText },
                PreIW = x.PreIW == null ? null : new Wording() { WordID = x.PreIW.ID, WordingText = x.PreIW.WordingText },
                PreAW = x.PreAW == null ? null : new Wording() { WordID = x.PreAW.ID, WordingText = x.PreAW.WordingText },
                LitQW = x.LitQW == null ? null : new Wording() { WordID = x.LitQW.ID, WordingText = x.LitQW.WordingText },
                PstIW = x.PstIW == null ? null : new Wording() { WordID = x.PstIW.ID, WordingText = x.PstIW.WordingText },
                PstPW = x.PstPW == null ? null : new Wording() { WordID = x.PstPW.ID, WordingText = x.PstPW.WordingText },
                RespOptionsS = x.RespOptionsS == null ? null : new ResponseSet() { RespSetName = x.RespOptionsS.RespSetName, RespList = x.RespOptionsS.RespList },
                NRCodesS = x.NRCodesS == null ? null : new ResponseSet() { RespSetName = x.NRCodesS.RespSetName, RespList = x.NRCodesS.RespList },
                SurveyList = x.SurveyList
            }).ToList();

            return questions;
        }
    }

       
    
}
