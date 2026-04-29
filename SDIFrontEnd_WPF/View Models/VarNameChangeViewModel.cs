using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmLib.ViewModels;
using ITCLib;

namespace SDIFrontEnd_WPF.ViewModels
{
    public class VarNameChangeViewModel : ViewModelBase
    {

        private readonly IApiPeopleService _peopleService;

        public string OldVarName { get; set; } = string.Empty;
        public string NewVarName { get; set; } = string.Empty;

        public string Authoriztion { get; set; } = string.Empty;
        public string Rationale { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public bool PreFWChange { get; set; } = false;
        public bool HiddenChange { get; set; } = false;

        public VarNameChange GetChangeObject => new VarNameChange
        {
            OldName = this.OldVarName,
            NewName = this.NewVarName,
            Authorization = this.Authoriztion,
            Rationale = this.Rationale,
            Source = this.Source,
            PreFWChange = this.PreFWChange,
            HiddenChange = this.HiddenChange
        };

        public List<Person> PeopleList { get; set; }

        public VarNameChangeViewModel(IApiPeopleService peopleService) 
        {
            _peopleService = peopleService;
            DisplayName = "Variable Name Change Details";
            _ = Load();
        }

        async Task Load()
        {
            PeopleList = await _peopleService.GetPeopleBasics();
        }
    }
}
