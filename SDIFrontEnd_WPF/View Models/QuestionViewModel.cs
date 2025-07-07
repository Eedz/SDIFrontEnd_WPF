using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmLib.ViewModels;
using ITCLib;

namespace SDIFrontEnd_WPF
{
    public class QuestionViewModel : ViewModelBase
    {
        readonly SurveyQuestion _question;

        public QuestionViewModel (SurveyQuestion question)
        {
            _question = question;
        }
    }
}
