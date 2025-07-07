using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using ITC_Services;
using ITCLib;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
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
        private readonly IUserService _userService;

        private UserPrefs CurrentUser;

        public IEnumerable<MenuCategory> MenuCategories => Enum.GetValues(typeof(MenuCategory)).Cast<MenuCategory>();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentSublinks))]
        public MenuCategory selectedMenuCategory = MenuCategory.Home;

        public IEnumerable<string> CurrentSublinks =>
       SelectedMenuCategory switch
       {
           MenuCategory.Home => new[] { "Home" },
           MenuCategory.Surveys => new[] { "Survey1", "Survey2", "Survey3" },
           MenuCategory.VarNames => new[] { "Rename", "Prefixes", "Labels" },
           MenuCategory.Search => new[] { "Questions", "Response Sets", "Comments" },
           MenuCategory.Praccing => new[] { "Entry", "Report", "Import", "Sheet", "Form" },
           MenuCategory.Reports => new[] { "Single", "Comparison", "Translation", "Website", "External", "Harmony", "Products", "Parallel Vars", "Topic/Content" },
           MenuCategory.Timing => new[] { "Survey Timing" },
           _ => Enumerable.Empty<string>()
       };

        [ObservableProperty]
        private string selectedSublink;

        partial void OnSelectedSublinkChanged(string value)
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
            _userService = _services.GetService(typeof(IUserService)) as IUserService ?? throw new ArgumentNullException(nameof(_services), "User service cannot be null");
            CurrentUser = _userService.GetUser(Environment.UserName) ?? throw new ArgumentNullException(nameof(_userService), "User preferences cannot be null");
        }

        private async Task<ViewModelBase> OpenSurveyManager(int index)
        {

            // get user's filter matching the index
            var surveyService = _services.GetService(typeof(ISurveyService)) as ISurveyService ?? throw new ArgumentNullException(nameof(_services), "Survey service cannot be null");
            Survey survey = new ITCLib.Survey(); 
            survey.SurveyCode = "TT1";
            survey.Title = "Test Survey";
            survey.Questions.Add(new SurveyQuestion("AA111", "0001"));
            survey = await surveyService.GetSurveyByIdAsync(944);
            var questions = surveyService.GetQuestionsForSurvey(survey.SID);
            survey.AddQuestions(questions);
            return new SurveyManagerViewModel(_services, survey);
        }

        private async void SetActiveForm()
        {
            ActiveForm = SelectedSublink switch
            {
                "Home" => new HomeViewModel(),
                "Survey1" => await OpenSurveyManager(1),
                "Survey2" => await OpenSurveyManager(2),
                "Survey3" => await OpenSurveyManager(3),
                "Questions" => new QuestionSearchViewModel(),
                _ => null
            };
        }
    }
}
