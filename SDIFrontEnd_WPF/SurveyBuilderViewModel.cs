using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;
using CommunityToolkit.Mvvm.ComponentModel;
using MvvmLib.ViewModels;
using CommunityToolkit.Mvvm.Input;
using ITC_Services;
namespace SDIFrontEnd_WPF
{
    public partial class SurveyBuilderViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IReferenceDataService _referenceDataService;
        private readonly ObservableCollection<SurveyQuestion> _questionList;

        public List<TopicLabel> TopicLabels { get; set; }
        public List<ContentLabel> ContentLabels { get; set; }
        public List<DomainLabel> DomainLabels { get; set; }
        public List<ProductLabel> ProductLabels { get; set; }

        public ObservableCollection<SurveyQuestion> QuestionList => _questionList;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentQuestionText))]
        private SurveyQuestion selectedQuestion;

        public string CurrentQuestionText => SelectedQuestion.GetQuestionTextHTML() ?? string.Empty;

        public SurveyBuilderViewModel(IDialogService dialogService, IReferenceDataService referenceData, IEnumerable<SurveyQuestion> questions)
        {
            _dialogService = dialogService;
            _referenceDataService = referenceData ?? throw new ArgumentNullException(nameof(referenceData), "Reference data service cannot be null.");
            if (questions == null) throw new ArgumentNullException(nameof(questions), "Questions cannot be null");

            _questionList = new ObservableCollection<SurveyQuestion>(questions);

            TopicLabels= _referenceDataService.GetTopicLabels() ?? new List<TopicLabel>();
            ContentLabels= _referenceDataService.GetContentLabels() ?? new List<ContentLabel>();
            DomainLabels= _referenceDataService.GetDomainLabels() ?? new List<DomainLabel>();
            ProductLabels= _referenceDataService.GetProductLabels() ?? new List<ProductLabel>();

            SelectedQuestion = _questionList.FirstOrDefault() ?? new SurveyQuestion("Default", "0000");
        }

        [RelayCommand]
        private void EditWording(WordingType wordingType)
        {
            // open wording editor dialog

        }

        [RelayCommand]
        private void EditResponseSet(ResponseSet responseSet) 
        { 
        }

        [RelayCommand]
        private void CopyPreviousQuestion()
        {

        }

        [RelayCommand]
        private void ViewComments()
        {

        }

        [RelayCommand]
        private void ViewTranslations()
        {
            if (SelectedQuestion == null)
            {
                _dialogService.ShowError("No question selected for translation.", "Translation Error");
                return;
            }
            var translations = SelectedQuestion.Translations;
            if (translations == null || !translations.Any())
            {
                _dialogService.ShowMessage("No translations available for this question.", "Translations");
                return;
            }
            // Assuming you have a dialog to show translations
            //var translationDialog = new TranslationDialog(translations);
            //translationDialog.ShowDialog();
        }
    }
}
