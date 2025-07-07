using ITCLib;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace SDIFrontEnd_WPF.DesignTimeViewModels
{
    public class SurveyManagerViewModel : ViewModelBase
    {
        public readonly Survey CurrentSurvey;
        public SurveyManagerViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                // Design-time data initialization
                CurrentSurvey = new Survey("TT1")
                {
                    Title = "Test Survey",

                };
                DisplayName = "Survey Manager - Design Time";
            }
            else
            {
                // Runtime data initialization
                CurrentSurvey = new Survey("TT1")
                {
                    Title = "Test Survey",
                };
                DisplayName = "Survey Manager - " + CurrentSurvey.SurveyCode;
            }
        }
    }
}
