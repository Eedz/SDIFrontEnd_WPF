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
    
    public partial class SurveyManagerViewModel :ViewModelBase
    {
        private readonly ISurveyService _surveyService;
        private readonly IReferenceDataService _referenceDataService; 
        private readonly IDialogService _dialogService;
        private readonly LookupProvider _lookupProvider; // Provides access to reference data like modes, user states, etc.

        [ObservableProperty]
        private Survey currentSurvey; // The currently selected survey being managed

        public List<Survey> AllSurveys { get; private set; } // List of all surveys available in the system

        public SurveyViewModel SurveyInfo { get; set; } // ViewModel for displaying basic survey information
        public SurveyBuilderViewModel SurveyBuilder { get; set; } // ViewModel for managing survey questions and their properties

        public SurveyViewModel? EditSurvey { get; set; } // TODO make this a different viewmodel for editing survey info



        public SurveyManagerViewModel(IServiceProvider services, Survey survey)
        {
            _surveyService = services.GetService(typeof(ISurveyService)) as ISurveyService ?? throw new ArgumentNullException(nameof(services), "Survey service cannot be null");
            _referenceDataService = services.GetService(typeof(IReferenceDataService)) as IReferenceDataService ?? throw new ArgumentNullException(nameof(services), "Reference data service cannot be null");
            _dialogService = services.GetService(typeof(IDialogService)) as IDialogService ?? throw new ArgumentNullException(nameof(services), "Dialog service cannot be null");
            _lookupProvider = services.GetService(typeof(LookupProvider)) as LookupProvider ?? throw new ArgumentNullException(nameof(services), "Lookup provider cannot be null");
            AllSurveys = _surveyService.GetAllSurveys();
            CurrentSurvey = survey ?? throw new ArgumentNullException(nameof(survey), "Survey cannot be null");
            DisplayName = "Survey Manager - " + CurrentSurvey.SurveyCode;
            SurveyInfo = new SurveyViewModel(CurrentSurvey);
            SurveyBuilder = new SurveyBuilderViewModel(CurrentSurvey.Questions);
        }

        partial void OnCurrentSurveyChanged(Survey value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Current survey cannot be null");
            
            DisplayName = "Survey Manager - " + value.SurveyCode;

            value.AddQuestions(_surveyService.GetQuestionsForSurvey(value.SID));

            SurveyInfo = new SurveyViewModel(value);
            SurveyBuilder = new SurveyBuilderViewModel(value.Questions);

            OnPropertyChanged(nameof(SurveyInfo));
            OnPropertyChanged(nameof(SurveyBuilder));
        }

        [RelayCommand]
        private void AddSurveyQuestion()
        {
            // enter VarName
            // if exists, ask user if they want to copy wordings or labels
            string newVarName = _dialogService.PromptForText("Enter VarName", "New Survey Question");

            if (string.IsNullOrWhiteSpace(newVarName))
                return;

            var existingQuestions = _surveyService.FindQuestionsByRefVarName(newVarName);
            SurveyQuestion selectedSource = null;

            if (existingQuestions.Count > 0)
            {
                selectedSource = _dialogService.PickQuestion(existingQuestions);

                SurveyQuestion newQuestion;

                if (selectedSource == null)
                { 
                    newQuestion = new SurveyQuestion(newVarName);
                    newQuestion.SurveyCode = CurrentSurvey.SurveyCode;
                }
                else
                {
                    newQuestion = selectedSource;
                    newQuestion.SurveyCode = CurrentSurvey.SurveyCode;
                    newQuestion.VarName.VarName = Utilities.ChangeCC(newVarName, CurrentSurvey.CountryCode);
                    newQuestion.Qnum = "0";
                }
                    

                CurrentSurvey.Questions.Add(newQuestion);
            }
            else
            {
                var newQuestion = new SurveyQuestion(newVarName);
                newQuestion.SurveyCode = CurrentSurvey.SurveyCode;
                
                

                CurrentSurvey.Questions.Add(newQuestion);
            }
           
           // SurveyBuilder.Refresh(); // assumes method to refresh the view
        }

        [RelayCommand]
        private void RemoveSurveyQuestion() {
            // ask user to document
            // ask user to save comments
            // delete
            CurrentSurvey.RemoveQuestion(SurveyBuilder.SelectedQuestion);
            
        }

        [RelayCommand]
        private void SaveChanges()
        {
            
        }

        [RelayCommand]
        private void LoadSurvey(string surveyCode)
        {

        }

        [RelayCommand]
        private void EditSurveyInfo() 
        {
            var editorVM = new SurveyEditorViewModel(CurrentSurvey.Clone(), _lookupProvider); // Clone to avoid editing original directly

            bool? result = _dialogService.ShowDialog(editorVM);

            if (result == true)
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
                _surveyService.UpdateSurvey(editorVM.Survey);
            }

        }
    }
}
