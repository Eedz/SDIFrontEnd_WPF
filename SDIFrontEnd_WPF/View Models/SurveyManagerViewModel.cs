using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITCLib;
using ITCReportLib;
using MvvmLib.ViewModels;
using SDIFrontEnd_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SDIFrontEnd_WPF
{

    public partial class SurveyManagerViewModel : ViewModelBase
    {
        private readonly IApiSurveyService _surveyService;
        private readonly IApiQuestionService _questionService; // Service for managing questions and their properties
        private readonly IDialogService _dialogService;
        private readonly ReferenceDataStore _referenceDataService;
        private readonly WordingData _wordingData;
        private readonly IApiWordingService _wordingService; // Service for managing question wordings and translations
       
        private readonly IApiPeopleService _peopleService; // Service for managing people data
        private readonly IApiCommentService _commentService; // Service for managing comments
        private readonly IWindowService _windowService; // Service for managing windows and dialogs

        [ObservableProperty]
        private Survey? currentSurvey; // The currently selected survey

        public List<Survey>? AllSurveys { get; private set; } // List of all surveys

        public SurveyViewModel? SurveyInfo { get; set; } // ViewModel for displaying basic survey information
        public SurveyBuilderViewModel? SurveyBuilder { get; set; } // ViewModel for managing survey questions and their properties

        public SurveyManagerViewModel(IApiSurveyService surveyService, IApiQuestionService questionService, IDialogService dialogService, ReferenceDataStore referenceDataService, WordingData wordingData,
            IApiWordingService wordingService, IApiPeopleService peopleService, IApiCommentService commentService, IWindowService windowService)
        {
            _surveyService = surveyService;
            _questionService = questionService;
            _dialogService = dialogService;
            
            _referenceDataService = referenceDataService;
            _wordingService = wordingService;
            _peopleService = peopleService;
            _commentService = commentService;
            _windowService = windowService;
            _wordingData = wordingData;

        }

        public async void Load(Survey survey)
        {
            if (survey == null)
                throw new ArgumentNullException(nameof(survey), "Survey cannot be null");
            
            CurrentSurvey = survey;

            AllSurveys = await _surveyService.GetAllAsync();
            OnPropertyChanged(nameof(AllSurveys));
            await LoadSurvey(survey);
        }

        partial void OnCurrentSurveyChanged(Survey value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Current survey cannot be null");

            _ = LoadSurvey(value);
        }

        [RelayCommand]
        private async Task LoadSurvey(Survey survey)
        {
            DisplayName = "Survey Manager - " + survey.SurveyCode;
            var questions = await _surveyService.GetSurveyQuestions(survey.SID);
            survey.AddQuestions(questions);
            
            SurveyInfo = new SurveyViewModel(survey);
            SurveyBuilder = new SurveyBuilderViewModel(_dialogService, _surveyService, _questionService, _referenceDataService, _wordingService, _peopleService, _commentService, _wordingData, survey);

            OnPropertyChanged(nameof(SurveyInfo));
            OnPropertyChanged(nameof(SurveyBuilder));
        }

        [RelayCommand]
        private async Task EditSurveyInfo()
        {
            var survey = await _surveyService.GetSurveyByIdAsync(CurrentSurvey.SID);
            var editorVM = new SurveyEditorViewModel(survey, _referenceDataService); 

            bool? save = _dialogService.ShowDialog(editorVM);

            if (save == true)
            {
                var saved = await _surveyService.UpdateSurvey(editorVM.Survey);
                CurrentSurvey = saved;
              
                SurveyInfo = new SurveyViewModel(editorVM.Survey);
                OnPropertyChanged(nameof(SurveyInfo));
            }
        }

        [RelayCommand]
        private void ImportTranslations() 
        {
            TranslationImporterViewModel vm = new TranslationImporterViewModel(_surveyService, _questionService, _referenceDataService, _dialogService);
            Task.Run(()=>vm.LoadAsync());
            vm.SelectedSurvey = CurrentSurvey;
            vm.SelectedLanguage = CurrentSurvey.LanguageList.FirstOrDefault()?.SurvLanguage;
            _dialogService.ShowWindow(vm);
        }

        [RelayCommand]
        private void ImportQuestions() 
        {
            this._windowService.ShowQuestionImporterWindow();
        }

        [RelayCommand]
        private void ExportSAS()
        {
            SyntaxReport report = new SyntaxReport();
            report.UseQnum = true;
            report.OutputPath = @"\\psychfile\psych$\psych-lab-gfong\SMG\SDI\Data Templates\";
            report.CreateSyntax(CurrentSurvey, SyntaxFormat.SAS);
            _dialogService.ShowMessage("SAS syntax exported successfully.", "Export Complete");
        }

        [RelayCommand]  
        private void ExportQuestions()
        {
            

        }
    }
}
