using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITC_Services;
using ITCLib;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
namespace SDIFrontEnd_WPF
{
    public enum MenuCategory { Home, Surveys, VarNames, Search, Praccing, Reports, Timing }
    public partial class MainWindowViewModel : ViewModelBase
    {
        // this class controls which items are displayed in the left pane of the main window
        // when an item is selected, the corresponding user control is displayed in the right pane

        private readonly IServiceProvider _services; // Assuming you have a service to manage surveys
        private readonly ISurveyService _surveyService; // Assuming you have a service to manage surveys
        private readonly IUserService _userService;

        private UserPrefs CurrentUser;

        public IEnumerable<MenuCategory> MenuCategories => Enum.GetValues(typeof(MenuCategory)).Cast<MenuCategory>();

        public List<Survey> AvailableSurveysToAdd { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentSublinks))]
        public MenuCategory selectedMenuCategory = MenuCategory.Home;

        [ObservableProperty]
        private ObservableCollection<SublinkItem> currentSublinks;
           
        [ObservableProperty]
        private SublinkItem selectedSublink;
        [ObservableProperty]
        private Survey selectedNewSurvey;

        public bool IsSurveyCategory => SelectedMenuCategory == MenuCategory.Surveys;

        partial void OnSelectedMenuCategoryChanged(MenuCategory value)
        {
            LoadSublinks(value);
            OnPropertyChanged(nameof(IsSurveyCategory));
        }

        partial void OnSelectedSublinkChanged(SublinkItem value)
        {
            // TODO decide if viewmodels should be disposed
            //if (ActiveForm is IDisposable disposable)
            //    disposable.Dispose();

            if (value == null && SelectedMenuCategory == MenuCategory.Home)
            {
                ActiveForm = new HomeViewModel(); // Reset to HomeViewModel if no sublink is selected
                return;
            }
            SetActiveForm();
           
        }

        [ObservableProperty]
        public ViewModelBase? activeForm;

        public MainWindowViewModel(IServiceProvider services)
        {
            DisplayName = "Main Window ViewModel";
            ActiveForm = new HomeViewModel(); // Set the initial active form to HomeViewModel
            _services = services;
            _surveyService = _services.GetService(typeof(ISurveyService)) as ISurveyService ?? throw new ArgumentNullException(nameof(_services), "Survey service cannot be null");
            _userService = _services.GetService(typeof(IUserService)) as IUserService ?? throw new ArgumentNullException(nameof(_services), "User service cannot be null");
            CurrentUser = _userService.GetUser(Environment.UserName) ?? throw new ArgumentNullException(nameof(_userService), "User preferences cannot be null");
            AvailableSurveysToAdd = _surveyService.GetAllSurveys();
            CurrentSublinks = new ObservableCollection<SublinkItem>();
        }

        private async Task<ViewModelBase> OpenSurveyManager(int surveyId)
        {
            // get user's filter matching the index
           // int surveyId = CurrentUser.GetFilterID("frmSurveyEntry", index);
            Survey survey = await _surveyService.GetSurveyByIdAsync(surveyId);
            var questions = _surveyService.GetQuestionsForSurvey(surveyId);
            survey.AddQuestions(questions);
            return new SurveyManagerViewModel(_services, survey);
        }

        private async void SetActiveForm()
        {
            if (SelectedSublink == null)
            {
                ActiveForm = new HomeViewModel(); // Reset to HomeViewModel if no sublink is selected
                return;
            }

            if (SelectedSublink is SurveySublinkItem surveySublink)
            {
                // If the selected sublink is a survey, open the survey manager
                ActiveForm = await OpenSurveyManager(surveySublink.SurveyId);
                return;
            }else
            {
                ActiveForm = SelectedSublink.Key switch
                {
                    "home" => new HomeViewModel(),
                    "Questions" => new QuestionSearchViewModel(),
                    _ => null
                };
            }

                
        }

        private ObservableCollection<SublinkItem> GetSurveyEditorList()
        {
            var list = Enumerable.Range(1, 3)
                .Select(i => CurrentUser.GetFilterID("frmSurveyEntry", i))
                .ToList();

            ObservableCollection<SublinkItem> surveyList = new ObservableCollection<SublinkItem>();
            foreach (int index in list) {
                var survey = _surveyService.GetSurveyById(index);
                surveyList.Add(new SurveySublinkItem(survey.SurveyCode, "survey_" + index, MenuCategory.Surveys, survey.SID));
            }

            return surveyList;
        }
        private ObservableCollection<SublinkItem> GetVarNameLinks()
        {
            return new ObservableCollection<SublinkItem>()
            {
                new SublinkItem ("Rename Vars", "rename_vars", MenuCategory.VarNames),
                new SublinkItem("VarName Changes", "varchanges", MenuCategory.VarNames),
                new SublinkItem("VarName Usage", "varusage", MenuCategory.VarNames),
                new SublinkItem("Prefix List", "prefixes", MenuCategory.VarNames),
                new SublinkItem("VarName History", "varhistory", MenuCategory.VarNames),
            };
        }

        private ObservableCollection<SublinkItem> GetSearchLinks()
        {
            return new ObservableCollection<SublinkItem>()
            {
                new SublinkItem("Questions", "Questions", MenuCategory.Search),
                new SublinkItem("Response Sets", "ResponseSets", MenuCategory.Search),
                new SublinkItem("Comments", "Comments", MenuCategory.Search),
            };
        }

        private ObservableCollection<SublinkItem> GetPraccingLinks()
        {
            return new ObservableCollection<SublinkItem>()
            {
                new SublinkItem("Entry", "Entry", MenuCategory.Praccing),
                new SublinkItem("Report", "Report", MenuCategory.Praccing),
                new SublinkItem("Import", "Import", MenuCategory.Praccing),
                new SublinkItem("Sheet", "Sheet", MenuCategory.Praccing),
                new SublinkItem("Form", "Form", MenuCategory.Praccing),
            };
        }

        private ObservableCollection<SublinkItem> GetReportLinks()
        {
            return new ObservableCollection<SublinkItem>()
            {
                new SublinkItem("Single", "Single", MenuCategory.Praccing),
                new SublinkItem("Comparison", "Comparison", MenuCategory.Praccing),
                new SublinkItem("Translation", "Translation", MenuCategory.Praccing),
                new SublinkItem("Website", "Website", MenuCategory.Praccing),
                new SublinkItem("External", "External", MenuCategory.Praccing),
                new SublinkItem("Harmony", "Harmony", MenuCategory.Praccing),
                new SublinkItem("Parallel Vars", "Parallel Vars", MenuCategory.Praccing),
                new SublinkItem("Topic/Content", "Topic/Content", MenuCategory.Praccing),
                
            };
        }

        private void LoadSublinks(MenuCategory category)
        { 
            // This method can be used to load sublinks based on the selected category
            // For now, it just sets the SelectedSublink to null to reset the view
            CurrentSublinks.Clear();

            IEnumerable<SublinkItem> items = category switch
            {
                MenuCategory.Home => new[] { new SublinkItem("Home", "home", category) },
                MenuCategory.Surveys => GetSurveyEditorList(),
                MenuCategory.VarNames => GetVarNameLinks(),
                MenuCategory.Search => GetSearchLinks(),
                MenuCategory.Praccing => GetPraccingLinks(),
                MenuCategory.Reports => GetReportLinks(),
                MenuCategory.Timing => new ObservableCollection<SublinkItem>() { new SublinkItem("Timing", "Timing", MenuCategory.Timing)},
                _ => Enumerable.Empty<SublinkItem>()
            };

            foreach (var item in items)
                CurrentSublinks.Add(item);
        }

        [RelayCommand]
        private void AddSelectedSurvey()
        {
            if (SelectedNewSurvey is null || CurrentSublinks.Any(x=>x.Key.Equals("survey_" + SelectedNewSurvey.SID)))
                return;

            CurrentSublinks.Add(
                new SurveySublinkItem(SelectedNewSurvey.SurveyCode, "survey_" + SelectedNewSurvey.SID, MenuCategory.Surveys, SelectedNewSurvey.SID  ));
            SelectedNewSurvey = null; // reset drop down
            
        }
    }
}
