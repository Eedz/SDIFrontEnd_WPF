using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITCLib;
using ITCReportLib;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class PraccingReportViewModel : ViewModelBase
    {
        private readonly IApiPraccingService _praccingService;

        // lists
        public IEnumerable<Survey>? SurveyList { get; private set; }
        public List<string>? StatusList { get; private set; }
       
        public List<string>? SortOptions { get; private set; }

        public List<PraccingIssue>? IssueList { get; private set; }

        public SelectableList<string>? DateList { get; private set; }
        public SelectableList<Person>? FromList { get; private set; }

        public SelectableList<Person>? ToList { get; private set; }
        public SelectableList<PraccingCategory>? CategoryList { get; private set; }
        public SelectableList<string>? LastUpdateDateList { get; private set; }
        public SelectableList<Person>? LastUpdateFromList { get; private set; }
        public SelectableList<Person>? LastUpdateToList { get; private set; }

        public ObservableCollection<string>? LanguageList { get; private set; }

        // selected items
        [ObservableProperty]
        private Survey? selectedSurvey;
        public string? SelectedSortOption { get; set; }
        public string? SelectedStatus { get; set; }
        
        
        
        public string? SelectedLanguage { get; set; }

        


        // options
        public bool IncludeInstructions { get; set; } = true;
        public bool IncludeQnums { get; set; }
        public bool IncludeEmptyRow { get; set; } = true;
        public bool IncludePrevNames { get; set; }

        public PraccingReportViewModel(IApiPraccingService praccingService) 
        { 
            _praccingService = praccingService ?? throw new ArgumentNullException(nameof(praccingService));
            DisplayName = "Praccing Report";
        }

        public async Task LoadSurveysAsync()
        {
            var surveys = await _praccingService.GetPraccingSurveys();
            SurveyList = surveys;
            StatusList = new List<string> { "Unresolved", "Resolved" };
            StatusList.Insert(0, "<All>");
            SelectedStatus = StatusList[1];
            LanguageList = ["<All>"];
            SortOptions = new List<string> { "IssueNo", "Last Update" };
            SelectedSortOption = "IssueNo";
            OnPropertyChanged(nameof(SurveyList));
        }

        async partial void OnSelectedSurveyChanged(Survey? oldValue, Survey? newValue)
        {
            if (newValue == null)
                return;
            IssueList = await _praccingService.GetPraccingIssues(newValue.SID);
            LoadData();
        }


        private void LoadData()
        {
            if (IssueList == null || IssueList.Count == 0)
                return;
        
            DateList = new SelectableList<string>(IssueList.Select(x=>x.IssueDate.Date).Distinct().OrderByDescending(x=>x).Select(x=>x.Date.ToString("ddMMMyyyy")).ToList());
            DateList.AllItem!.IsSelected = true;
            FromList = new SelectableList<Person>(IssueList.Select(x => x.IssueFrom).Distinct().ToList(), true);
            FromList.AllItem!.IsSelected = true;
            ToList = new SelectableList<Person>(IssueList.Select(x => x.IssueTo).Distinct().ToList());
            ToList.AllItem!.IsSelected = true;
            CategoryList = new SelectableList<PraccingCategory>(IssueList.Select(x => x.Category).Distinct().ToList());
            CategoryList.AllItem!.IsSelected = true;    
            GetLastUpdateInfo();

            LanguageList = new ObservableCollection<string>(IssueList.Select(x => x.Language).Distinct().ToList());
            LanguageList.Insert(0, "<All>");

            OnPropertyChanged(nameof(DateList));
            OnPropertyChanged(nameof(FromList));
            OnPropertyChanged(nameof(ToList));
            OnPropertyChanged(nameof(CategoryList));
            OnPropertyChanged(nameof(LastUpdateDateList));
            OnPropertyChanged(nameof(LastUpdateFromList));
            OnPropertyChanged(nameof(LastUpdateToList));
            OnPropertyChanged(nameof(LanguageList));
        }

        private void GetLastUpdateInfo()
        {
            if (IssueList == null || IssueList.Count == 0)
                return;
        

            var lastUpdates = IssueList
                .Select(issue =>
                {
                    var lastResponse = issue.Responses?
                        .OrderBy(r => r.ResponseDate)
                        .LastOrDefault();

                    return new
                    {
                        Date = lastResponse?.ResponseDate ?? issue.IssueDate,
                        FromName = lastResponse?.ResponseFrom ?? issue.IssueFrom,
                        ToName = lastResponse?.ResponseTo ?? issue.IssueTo
                    };
                })
                .ToList();

            var lastUpdateDates = lastUpdates
                .Select(x => x.Date.Date)
                .Distinct()
                .OrderByDescending(d => d).Select(x=>x.Date.ToString("ddMMMyyyy"))
                .ToList();

            var lastUpdateFromNames = lastUpdates
                .Select(x => x.FromName)
                .Distinct()
                .OrderBy(n => n.Name)
                .ToList();

            var lastUpdateToNames = lastUpdates
                .Select(x => x.ToName)
                .Distinct()
                .OrderBy(n => n.Name)
                .ToList();

            LastUpdateDateList = new SelectableList<string>(lastUpdateDates);
            LastUpdateDateList.AllItem!.IsSelected = true;
            LastUpdateFromList = new SelectableList<Person>(lastUpdateFromNames);
            LastUpdateFromList.AllItem!.IsSelected = true;
            LastUpdateToList = new SelectableList<Person>(lastUpdateToNames);
            LastUpdateToList.AllItem!.IsSelected = true;
        }

        private List<PraccingIssue> FilteredIssueList()
        {
            IEnumerable<PraccingIssue> issues = IssueList;
            if (SelectedStatus == "Resolved")
                issues = issues.Where(x => x.Resolved);
            else if (SelectedStatus == "Unresolved")
                issues = issues.Where(x => !x.Resolved);

            if (SelectedLanguage != null && SelectedLanguage != "<All>")
                issues = issues.Where(x => x.Language == SelectedLanguage);

            if (!DateList.IsAllSelected)
            {
                var selectedDateTimes = DateList.Items.Where(p => p.IsSelected).Select(d => DateTime.ParseExact(d.Value, "ddMMMyyyy", null).Date).ToList();
                issues = issues.Where(x => selectedDateTimes.Contains(x.IssueDate.Date));
            }

            if (!FromList.IsAllSelected)
                issues = issues.Where(x => FromList.Items.Any(p => p.IsSelected && p.Value.ID == x.IssueFrom.ID));

            if (!ToList.IsAllSelected)
                issues = issues.Where(x => ToList.Items.Any(p=>p.IsSelected && p.Value.ID == x.IssueTo.ID));

            if (!CategoryList.IsAllSelected)
                issues = issues.Where(x => CategoryList.Items.Any(p=>p.IsSelected && p.Value == x.Category));


            if (!LastUpdateDateList.IsAllSelected)
            {
                var selectedLUDates = LastUpdateDateList.Items.Where(p => p.IsSelected).Select(p => p.Value).ToList();
                issues = issues.Where(x => (x.Responses.Count() == 0 && selectedLUDates.Contains(x.IssueDate.ToString("d"))) ||
                    (x.Responses.Count() > 0 && selectedLUDates.Contains(x.Responses.Last().ResponseDate.Value.ToString("d"))));
            }

            if (!LastUpdateFromList.IsAllSelected)
            {
                var selectedLUFrom = LastUpdateFromList.Items.Where(p => p.IsSelected).Select(p => p.Value).ToList();
                issues = issues.Where(x => (x.Responses.Count() == 0 && selectedLUFrom.Contains(x.IssueFrom)) ||
                    (x.Responses.Count() > 0 && selectedLUFrom.Contains(x.Responses.Last().ResponseFrom)));
            }

            if (!LastUpdateToList.IsAllSelected)
            {
                var selectedLUTo = LastUpdateToList.Items.Where(p => p.IsSelected).Select(p => p.Value).ToList();
                issues = issues.Where(x => (x.Responses.Count() == 0 && selectedLUTo.Contains(x.IssueTo)) ||
                    (x.Responses.Count() > 0 && selectedLUTo.Contains(x.Responses.Last().ResponseTo)));
            }

            return issues.ToList();
        }

        List<string> GetRecipients()
        {
            var to =!ToList.IsAllSelected
                ? ToList.Items.Where(p => p.IsSelected).Select(p => p.Value.Name)
                : Enumerable.Empty<string>();

            var luTo = !LastUpdateToList.IsAllSelected
                ? LastUpdateToList.Items.Where(p => p.IsSelected).Select(p => p.Value.Name)
                : Enumerable.Empty<string>();

            return to
                .Concat(luTo)
                .Distinct()
                .ToList();
        }


        [RelayCommand]
        private void OpenReportsFolder()
        {
            System.Diagnostics.Process.Start("explorer.exe", @"\\psychfile\psych$\psych-lab-gfong\SMG\SDI\Reports\Praccing");
            
        }

        [RelayCommand]
        private void GenerateReport()
        {
            

            PraccingReport report = new PraccingReport();
            report.Issues = FilteredIssueList();
            report.SelectedSurvey = SelectedSurvey;
            report.IncludePraccInstructions = IncludeInstructions;
            report.IncludeQnums = IncludeQnums;
            report.AddBlankLine = IncludeEmptyRow;
            report.Recipients = GetRecipients();
            report.CreateReport();
            report.OutputReport();
        }

        

    }
}
