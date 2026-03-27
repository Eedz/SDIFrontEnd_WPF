using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ITCLib;
using ITCReportLib;
using Microsoft.Extensions.DependencyInjection;
using MvvmLib.ViewModels;
using SDIFrontEnd_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SDIFrontEnd_WPF
{
    public enum MenuCategory { Home, Surveys, VarNames, Search, Praccing, Reports, Timing }
    public partial class MainWindowViewModel : ViewModelBase
    {
        // this class controls which items are displayed in the left pane of the main window
        // when an item is selected, the corresponding user control is displayed in the right pane

        private readonly IServiceProvider _services; 
        private readonly IApiUserService _userService;
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
            ActiveForm = services.GetRequiredService<HomeViewModel>(); // Set the initial active form to HomeViewModel
            _services = services;
            _userService = _services.GetService(typeof(IApiUserService)) as IApiUserService ?? throw new ArgumentNullException(nameof(_services), "User service cannot be null");
            
            apiSurveyService = _services.GetService(typeof(IApiSurveyService)) as IApiSurveyService ?? throw new ArgumentNullException(nameof(_services), "Survey API service cannot be null");// Initialize the API survey service with the survey service
            
           
            CurrentSublinks = new ObservableCollection<SublinkItem>();
        }

        public async Task LoadAsync()
        {
            var surveys = await apiSurveyService.GetAllAsync();
            AvailableSurveysToAdd.AddRange(surveys);
            CurrentUser = await _userService.GetUser(Environment.UserName) ?? throw new ArgumentNullException(nameof(_userService), "User preferences cannot be null");            
            
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
                        ActiveForm =  await OpenPraccingView();
                        break;
                    case MenuCategory.Search:
                        ActiveForm = await OpenSearchView();
                        break;
                    case MenuCategory.VarNames:
                        ActiveForm = await OpenVarNameView();
                        break;
                    case MenuCategory.Reports:
                        ActiveForm = await OpenReportView();
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

        private async Task<ViewModelBase> OpenReportView()
        {
            ViewModelBase vm;
            switch (SelectedSublink.Key)
            {
                case "Harmony":
                    vm = _services.GetRequiredService<HarmonyReportViewModel>();
                    await ((HarmonyReportViewModel)vm).LoadDataAsync();
                    return vm;
                case "Variable List":
                    vm = _services.GetRequiredService<QuestionSurveyMatrixViewModel>();
                    await ((QuestionSurveyMatrixViewModel)vm).Load();
                    return vm;
            
                default:
                    return _services.GetRequiredService<HomeViewModel>();
            }
        }

        private async Task<ViewModelBase> OpenPraccingView()
        {
            ViewModelBase vm;

            switch(SelectedSublink.Key)
            {
                case "Entry":
                    vm = _services.GetRequiredService<PraccingEntryViewModel>();
                    await ((PraccingEntryViewModel)vm).Load();
                    return vm;
                case "Report":
                    vm = _services.GetRequiredService<PraccingReportViewModel>();
                    await ((PraccingReportViewModel)vm).LoadSurveysAsync();
                    return vm;
                case "Import":
                    vm = _services.GetRequiredService<PraccingImportViewModel>();
                   // await ((PraccingImportViewModel)vm).Load();
                    return vm;
                case "Sheet":
                    vm = _services.GetRequiredService<PraccingSheetViewModel>();
                    await ((PraccingSheetViewModel)vm).LoadSurveys();
                    return vm;
                case "Form":
                    PraccingReportBlank report = new PraccingReportBlank();
                    report.CreateReport();
                    report.OutputReport();
                    return null;
                default:
                    vm = _services.GetRequiredService<PraccingEntryViewModel>();
                    await ((PraccingEntryViewModel)vm).Load();
                    return vm;
            }
        }

        private async Task<ViewModelBase> OpenSearchView()
        {
            ViewModelBase vm;
            switch (SelectedSublink.Key)
            {
                case "Questions":
                    vm = _services.GetRequiredService<QuestionSearchViewModel>();
                    await ((QuestionSearchViewModel)vm).LoadSurveys();
                    return vm;
                //case "ResponseSets":
                //    return new ResponseSetSearchViewModel(_surveyService);
                //case "Comments":
                //    return new CommentSearchViewModel(_surveyService, _peopleService);
                default:
                    vm = _services.GetRequiredService<QuestionSearchViewModel>();
                    await ((QuestionSearchViewModel)vm).LoadSurveys();
                    return vm;
            }
        }

        private async Task<ViewModelBase> OpenVarNameView()
        {
            ViewModelBase vm;
            switch (SelectedSublink.Key)
            {
                case "varinfo":
                    vm = _services.GetRequiredService<VariableInformationViewModel>();
                    return vm;
                case "rename_vars":
                    vm = _services.GetRequiredService<RenameVarsViewModel>();
                    await ((RenameVarsViewModel)vm).LoadSurveysAsync();
                    return vm;
                    //case "varchanges":
                    //    return new VarNameChangesViewModel(_varnameService);
                    //case "varusage":
                    //    return new VarNameUsageViewModel(_varnameService, _surveyService);
                case "prefixes":
                    vm = _services.GetRequiredService<PrefixListViewModel>();
                    return vm;
                    
                case "history":
                    vm = _services.GetRequiredService<QuestionHistoryManagerViewModel>();
                    await ((QuestionHistoryManagerViewModel)vm).Load();
                    return vm;
                default:
                    return null;
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
                new SublinkItem("Variable Info", "varinfo", MenuCategory.VarNames),
                new SublinkItem("Rename Vars", "rename_vars", MenuCategory.VarNames),
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
