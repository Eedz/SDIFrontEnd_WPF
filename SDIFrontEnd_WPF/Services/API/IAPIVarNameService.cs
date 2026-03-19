using ITCLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    public interface IApiVarNameService
    {
        Task<int> InsertVariable(VariableName dto);
        Task<List<VariableName>> GetAllVarNames();
        Task<List<VariableName>> GetAllVarNamesByRefList(List<string> varlist);
        Task<VariableName> GetVariableInfo(string varname);
        Task<List<VariableName>> GetVariableInfoByRef(string refvarname);
        Task<List<VariableName>> SearchVarNames(string searchTerm, int take);
    }
}
