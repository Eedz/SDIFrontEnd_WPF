using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using ITC_Services;
using ITCLib;
using Microsoft.Extensions.DependencyInjection;
using MvvmLib.ViewModels;
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
        private readonly ISurveyService _surveyService;
        private readonly IDialogService _dialogService;
        private readonly IReferenceDataService _referenceDataService; 
        private readonly IWordingService _wordingService; // Service for managing question wordings and translations
        private readonly LookupProvider _lookupProvider; // Provides access to reference data like modes, user states, etc.

        [ObservableProperty]
        private Survey currentSurvey; // The currently selected survey being managed

        public List<Survey> AllSurveys { get; private set; } // List of all surveys available in the system

        public SurveyViewModel SurveyInfo { get; set; } // ViewModel for displaying basic survey information
        public SurveyBuilderViewModel SurveyBuilder { get; set; } // ViewModel for managing survey questions and their properties

        public SurveyManagerViewModel(IServiceProvider services, Survey survey)
        {
            _surveyService = services.GetService(typeof(ISurveyService)) as ISurveyService ?? throw new ArgumentNullException(nameof(services), "Survey service cannot be null");
            _dialogService = services.GetService(typeof(IDialogService)) as IDialogService ?? throw new ArgumentNullException(nameof(services), "Dialog service cannot be null");
            _lookupProvider = services.GetService(typeof(LookupProvider)) as LookupProvider ?? throw new ArgumentNullException(nameof(services), "Lookup provider cannot be null");
            _referenceDataService = services.GetService(typeof(IReferenceDataService)) as IReferenceDataService ?? throw new ArgumentNullException(nameof(services), "Reference data service cannot be null");
            _wordingService = services.GetService(typeof(IWordingService)) as IWordingService ?? throw new ArgumentNullException(nameof(services), "Wording service cannot be null");
            AllSurveys = _surveyService.GetAllSurveys();
            CurrentSurvey = survey ?? throw new ArgumentNullException(nameof(survey), "Survey cannot be null");
            DisplayName = "Survey Manager - " + CurrentSurvey.SurveyCode;
            SurveyInfo = new SurveyViewModel(CurrentSurvey);
            SurveyBuilder = new SurveyBuilderViewModel(_dialogService, _surveyService, _referenceDataService, _wordingService, CurrentSurvey);
        }

        partial void OnCurrentSurveyChanged(Survey value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Current survey cannot be null");

            DisplayName = "Survey Manager - " + value.SurveyCode;

            value.AddQuestions(_surveyService.GetQuestionsForSurvey(value.SID));

            SurveyInfo = new SurveyViewModel(value);
            SurveyBuilder = new SurveyBuilderViewModel(_dialogService, _referenceDataService, value.Questions);

            OnPropertyChanged(nameof(SurveyInfo));
            OnPropertyChanged(nameof(SurveyBuilder));
        }

        [RelayCommand]
        private void LoadSurvey(Survey survey)
        {
            DisplayName = "Survey Manager - " + survey.SurveyCode;

            survey.AddQuestions(_surveyService.GetQuestionsForSurvey(survey.SID));

            SurveyInfo = new SurveyViewModel(survey);
            SurveyBuilder = new SurveyBuilderViewModel(_dialogService, _surveyService, _referenceDataService, _wordingService, survey);

            OnPropertyChanged(nameof(SurveyInfo));
            OnPropertyChanged(nameof(SurveyBuilder));
                }

        [RelayCommand]
        private void EditSurveyInfo()
        {
            var editorVM = new SurveyEditorViewModel(CurrentSurvey.Clone(), _lookupProvider); // clone to avoid editing original directly

            bool? save = _dialogService.ShowDialog(editorVM);

            if (save == true)
            {
                var deletedStates = CurrentSurvey.UserStates.Except(editorVM.Survey.UserStates).ToList();
                var deletedProducts = CurrentSurvey.ScreenedProducts.Except(editorVM.Survey.ScreenedProducts).ToList();
                var deletedLanguages = CurrentSurvey.LanguageList.Except(editorVM.Survey.LanguageList).ToList();

                var addedStates = editorVM.Survey.UserStates.Except(CurrentSurvey.UserStates).ToList();
                var addedProducts = editorVM.Survey.ScreenedProducts.Except(CurrentSurvey.ScreenedProducts).ToList();
                var addedLanguages = editorVM.Survey.LanguageList.Except(CurrentSurvey.LanguageList).ToList();

                _surveyService.DeleteSurveyUserStates(deletedStates);
                _surveyService.DeleteSurveyScreenedProducts(deletedProducts);
                _surveyService.DeleteSurveyLanguages(deletedLanguages);

                _surveyService.AddSurveyUserStates(addedStates);
                _surveyService.AddSurveyScreenedProducts(addedProducts);
                _surveyService.AddSurveyLanguages(addedLanguages);

                // save changes back to the current survey
                //_surveyService.UpdateSurvey(editorVM.Survey);
                deletedStates.ForEach(x => CurrentSurvey.UserStates.Remove(x));
                deletedProducts.ForEach(x => CurrentSurvey.ScreenedProducts.Remove(x));
                deletedLanguages.ForEach(x => CurrentSurvey.LanguageList.Remove(x));

                addedStates.ForEach(x => CurrentSurvey.UserStates.Add(x));
                addedProducts.ForEach(x => CurrentSurvey.ScreenedProducts.Add(x));
                addedLanguages.ForEach(x => CurrentSurvey.LanguageList.Add(x));

                OnPropertyChanged(nameof(CurrentSurvey));
                SurveyInfo = new SurveyViewModel(editorVM.Survey);
                OnPropertyChanged(nameof(SurveyInfo));

        }

        }
    }
}
