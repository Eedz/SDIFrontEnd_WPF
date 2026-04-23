using ITCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    public interface IApiLabelService
    {
        Task<List<VariableName>> GetVarNamesByLabel(string type, VarNameLabel label);    
    }
}
