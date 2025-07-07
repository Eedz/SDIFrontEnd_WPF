using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;
using CommunityToolkit.Mvvm.ComponentModel;
using MvvmLib.ViewModels;
namespace SDIFrontEnd_WPF
{
    public partial class SurveyBuilderViewModel : ViewModelBase
    {
        private readonly ObservableCollection<SurveyQuestion> _questionList;
        public ObservableCollection<SurveyQuestion> QuestionList => _questionList;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentQuestionText))]
        private SurveyQuestion selectedQuestion;

        public string CurrentQuestionText => SelectedQuestion.GetQuestionTextPlain() ?? string.Empty;

        public SurveyBuilderViewModel(IEnumerable<SurveyQuestion> questions)
        {
            if (questions == null) throw new ArgumentNullException(nameof(questions), "Questions cannot be null");
            _questionList = new ObservableCollection<SurveyQuestion>(questions);
            SelectedQuestion = _questionList.FirstOrDefault() ?? new SurveyQuestion("Default", "0000");
        }
    }
}
