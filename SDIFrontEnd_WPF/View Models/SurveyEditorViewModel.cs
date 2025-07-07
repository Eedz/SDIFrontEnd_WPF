using CommunityToolkit.Mvvm.ComponentModel;
using ITCLib;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    public partial class SurveyEditorViewModel : ViewModelBase
    {
        private readonly Survey _survey;

        public IReadOnlyList<SurveyMode> Modes { get; set; }
        public IReadOnlyList<UserState> UserStates { get; set; }
        public IReadOnlyList<ScreenedProduct> ScreenedProducts { get; set; }
        public IReadOnlyList<Language> Languages { get;set; }
        public Survey Survey => _survey;


        public SurveyEditorViewModel(Survey survey, LookupProvider lookup)
        {
            DisplayName = "Survey - " + survey.SurveyCode;
            _survey = survey;
            Modes = lookup.Modes;
            UserStates = lookup.UserStates;
            ScreenedProducts = lookup.ScreenedProducts;
            Languages = lookup.Languages;
        }

        
    }
}
