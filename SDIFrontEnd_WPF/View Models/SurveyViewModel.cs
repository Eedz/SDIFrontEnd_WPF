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
    public partial class SurveyViewModel : ViewModelBase
    {
        private readonly Survey _survey;

        public Survey Survey => _survey;

        public string UserStates => string.Join(", ", _survey.UserStates.Select(us => us.State));
        public string ScreenedProducts => string.Join(", ", _survey.ScreenedProducts.Select(sp => sp.Product));
        public string Languages => _survey.LanguagesList;


        public SurveyViewModel(Survey survey)
        {
            DisplayName = "Survey - " + survey.SurveyCode;
            _survey = survey;
        }
    }
}
