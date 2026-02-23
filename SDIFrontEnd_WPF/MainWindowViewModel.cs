using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITC_Services;
using ITCLib;
using ITCReportLib;
using Microsoft.Extensions.DependencyInjection;
using MvvmLib.ViewModels;
using SDIFrontEnd_WPF.ViewModels;
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
        
        private readonly IServiceProvider _services; 
       // private readonly ISurveyService _surveyService; 
        private readonly IUserService _userService;
        private readonly IApiSurveyService apiSurveyService; // Service for managing surveys via API calls

        private UserPrefs CurrentUser;

        public IEnumerable<MenuCategory> MenuCategories => Enum.GetValues(typeof(MenuCategory)).Cast<MenuCategory>();

        public List<Survey> AvailableSurveysToAdd { get; } = new List<Survey>();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentSublinks))]
        public MenuCategory selectedMenuCategory = MenuCategory.Home;

        [ObservableProperty]
        private ObservableCollection<SublinkItem> currentSublinks;
           
        [ObservableProperty]
        private SublinkItem? selectedSublink;
        [ObservableProperty]
        private Survey? selectedNewSurvey;

        public bool IsSurveyCategory => SelectedMenuCategory == MenuCategory.Surveys;

        [ObservableProperty]
        public ViewModelBase? activeForm;

        public MainWindowViewModel(IServiceProvider services)
        {
            DisplayName = "Main Window ViewModel";
            ActiveForm = new HomeViewModel(); // Set the initial active form to HomeViewModel
            _services = services;
            _userService = _services.GetService(typeof(IUserService)) as IUserService ?? throw new ArgumentNullException(nameof(_services), "User service cannot be null");
            
            apiSurveyService = _services.GetService(typeof(IApiSurveyService)) as IApiSurveyService; // Initialize the API survey service with the survey service
            CurrentUser = _userService.GetUser(Environment.UserName) ?? throw new ArgumentNullException(nameof(_userService), "User preferences cannot be null");
           
            // Load surveys from the API instead of the local service
            CurrentSublinks = new ObservableCollection<SublinkItem>();

            _ = LoadAsync(); // Load surveys asynchronously when the ViewModel is initialized
        }

        private async Task LoadAsync()
        {
            var surveys = await apiSurveyService.GetAllAsync();
            AvailableSurveysToAdd.AddRange(surveys);
        }

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
                ActiveForm = _services.GetRequiredService<HomeViewModel>(); // reset to HomeViewModel if no sublink is selected
                return;
            }
            SetActiveForm();
        }

        private async void SetActiveForm()
        {
            if (SelectedSublink == null || SelectedSublink.Key=="home")
            {
                ActiveForm = _services.GetRequiredService<HomeViewModel>(); // reset to HomeViewModel if no sublink is selected
                return;
            }
         
            if (SelectedSublink is SurveySublinkItem surveySublink)
            {
                // If the selected sublink is a survey, open the survey manager
                ActiveForm = await OpenSurveyManager(surveySublink.SurveyId);
                return;
            }else
            {
                switch (SelectedSublink.Category)
                {
                    case MenuCategory.Surveys:

                        ActiveForm = await OpenSurveyManager(((SurveySublinkItem)SelectedSublink).SurveyId);
                        break;
                    case MenuCategory.Praccing:
                        ActiveForm = OpenPraccingView();
                        break;
                    case MenuCategory.Search:
                        ActiveForm = OpenSearchView();
                        break;
                    case MenuCategory.VarNames:
                        ActiveForm = OpenVarNameView();
                        break;
                    case MenuCategory.Reports:
                        ActiveForm = OpenReportView();
                        break;
                    default:
                        break;
                }
            }
        }

        private async Task<ViewModelBase> OpenSurveyManager(int surveyId)
        {
            Survey survey = await apiSurveyService.GetSurveyByIdAsync(surveyId);
            var vm = _services.GetRequiredService<SurveyManagerViewModel>();
            vm.Load(survey);
            return vm;
        }

        private ViewModelBase OpenReportView()
        {
            switch (SelectedSublink.Key)
            {
                case "Harmony":
                    return _services.GetRequiredService<HarmonyReportViewModel>();
                case "Variable List":
                    return _services.GetRequiredService<QuestionSurveyMatrixViewModel>();
            
                default:
                    return null;
            }
        }

        private ViewModelBase OpenPraccingView()
        {
            switch(SelectedSublink.Key)
            {
                case "Entry":
                    return _services.GetRequiredService<PraccingEntryViewModel>(); 
                case "Report":
                    return _services.GetRequiredService<PraccingReportViewModel>();
                case "Import":
                    return _services.GetRequiredService<PraccingImportViewModel>();
                case "Sheet":
                    return _services.GetRequiredService<PraccingSheetViewModel>();
                case "Form":
                    PraccingReportBlank report = new PraccingReportBlank();
                    report.CreateReport();
                    report.OutputReport();
                    return null;
                default:
                    return _services.GetRequiredService<PraccingEntryViewModel>();
            }
        }

        private ViewModelBase OpenSearchView()
        {
            switch (SelectedSublink.Key)
            {
                case "Questions":
                    return _services.GetRequiredService<QuestionSearchViewModel>();
                //case "ResponseSets":
                //    return new ResponseSetSearchViewModel(_surveyService);
                //case "Comments":
                //    return new CommentSearchViewModel(_surveyService, _peopleService);
                default:
                    return _services.GetRequiredService<QuestionSearchViewModel>();
            }
        }

        private ViewModelBase OpenVarNameView()
        {
            switch (SelectedSublink.Key)
            {
                case "rename_vars":
                    return _services.GetRequiredService<RenameVarsViewModel>();
                    //case "varchanges":
                    //    return new VarNameChangesViewModel(_varnameService);
                    //case "varusage":
                    //    return new VarNameUsageViewModel(_varnameService, _surveyService);
                    //case "prefixes":
                    //    return new PrefixListViewModel(_varnameService);
                    //case "varhistory":
                    //    return new VarNameHistoryViewModel(_varnameService);

                    break;
                case "history":
                    return _services.GetRequiredService<QuestionHistoryManagerViewModel>();
                default:
                    return null;//return new VarNameRenameViewModel(_varnameService, _surveyService);
            }
        }

        private async Task<ObservableCollection<SublinkItem>> GetSurveyEditorList()
        {
            var list = Enumerable.Range(1, 3)
                .Select(i => CurrentUser.GetFilterID("frmSurveyEntry", i))
                .ToList();

            ObservableCollection<SublinkItem> surveyList = new ObservableCollection<SublinkItem>();
            foreach (int index in list) {
                var survey = await apiSurveyService.GetSurveyByIdAsync(index);
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
                new SublinkItem("VarName History", "history", MenuCategory.VarNames),
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
                new SublinkItem("Single", "Single", MenuCategory.Reports),
                new SublinkItem("Comparison", "Comparison", MenuCategory.Reports),
                new SublinkItem("Translation", "Translation", MenuCategory.Reports),
                new SublinkItem("Website", "Website", MenuCategory.Reports),
                new SublinkItem("External", "External", MenuCategory.Reports),
                new SublinkItem("Variable List", "Variable List", MenuCategory.Reports),
                new SublinkItem("Harmony", "Harmony", MenuCategory.Reports),
                new SublinkItem("Parallel Vars", "Parallel Vars", MenuCategory.Reports),
                new SublinkItem("Topic/Content", "Topic/Content", MenuCategory.Reports),
                
            };
        }

        private async void  LoadSublinks(MenuCategory category)
        { 
            // This method can be used to load sublinks based on the selected category
            // For now, it just sets the SelectedSublink to null to reset the view
            CurrentSublinks.Clear();

            IEnumerable<SublinkItem> items = category switch
            {
                MenuCategory.Home => new[] { new SublinkItem("Home", "home", category) },
                MenuCategory.Surveys => await GetSurveyEditorList(),
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
