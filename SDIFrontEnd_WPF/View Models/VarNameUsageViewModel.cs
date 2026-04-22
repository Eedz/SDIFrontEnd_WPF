using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmLib.ViewModels;
using ITCLib;
namespace SDIFrontEnd_WPF.ViewModels
{
    public class VarNameUsageViewModel : WorkspaceViewModel
    {
        public List<QuestionUsage> Records { get; set; }

        public VarNameUsageViewModel(List<QuestionUsage> varnames) 
        {
            Records = varnames;
        }


    }
}
