using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITC_Services;
using ITCLib;
using ITCReportLib;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class PraccingReportViewModel : ViewModelBase
    {
        private readonly IPraccingService _praccingService;

        // lists
        public List<Survey> SurveyList { get; private set; }
        public List<string> StatusList { get; private set; }
       
        public List<string> SortOptions { get; private set; }

        public List<PraccingIssue> IssueList { get; private set; }

        public ObservableCollection<string> DateList { get; private set; }
        public ObservableCollection<Person> FromList { get; private set; }
        public ObservableCollection<Person> ToList { get; private set; }
        public ObservableCollection<PraccingCategory> CategoryList { get; private set; }
        public ObservableCollection<string> LastUpdateDateList { get; private set; }
        public ObservableCollection<Person> LastUpdateFromList { get; private set; }
        public ObservableCollection<Person> LastUpdateToList { get; private set; }

        public ObservableCollection<string> LanguageList { get; private set; }

        // selected items
        [ObservableProperty]
        private Survey selectedSurvey;
        public string SelectedSortOption { get; set; }
        public string SelectedStatus { get; set; }
        public ObservableCollection<string> SelectedDates { get; set; } = new();
        public ObservableCollection<Person> SelectedFrom { get; set; } = new();
        public ObservableCollection<Person> SelectedTo { get; set; } = new();
        public ObservableCollection<PraccingCategory> SelectedCategory { get; set; } = new();
        public ObservableCollection<string> SelectedLUDates { get; set; } = new();
        public ObservableCollection<Person> SelectedLUFrom { get; set; } = new();
        public ObservableCollection<Person> SelectedLUTo { get; set; } = new();
        public string SelectedLanguage { get; set; }

        // options
        public bool IncludeInstructions { get; set; } = true;
        public bool IncludeQnums { get; set; }
        public bool IncludeEmptyRow { get; set; } = true;
        public bool IncludePrevNames { get; set; }

        public PraccingReportViewModel(IPraccingService praccingService) 
        { 
            _praccingService = praccingService ?? throw new ArgumentNullException(nameof(praccingService));
            DisplayName = "Praccing Report";

            SurveyList = _praccingService.GetPraccingSurveys();
            StatusList = new List<string> { "Unresolved", "Resolved" };
            StatusList.Insert(0, "<All>");
            SelectedStatus = StatusList[1];
            LanguageList = ["<All>"];
            SortOptions = new List<string> { "IssueNo", "Last Update" };
            SelectedSortOption = "IssueNo";
        }

        partial void OnSelectedSurveyChanged(Survey? oldValue, Survey newValue)
        {
            IssueList = _praccingService.GetPraccingIssues(SelectedSurvey.SID);
            LoadData();
        }


        private void LoadData()
        {
            DateList = new ObservableCollection<string>(IssueList.Select(x=>x.IssueDate.Date).Distinct().OrderByDescending(x=>x).Select(x=>x.Date.ToString("ddMMMyyyy")).ToList());
            DateList.Insert(0, "<All>");          
            FromList = new ObservableCollection<Person>(IssueList.Select(x => x.IssueFrom).Distinct().ToList());
            FromList.Insert(0, new Person("<All>", -1));
            ToList = new ObservableCollection<Person>(IssueList.Select(x => x.IssueTo).Distinct().ToList());
            ToList.Insert(0, new Person("<All>", -1));
            CategoryList = new ObservableCollection<PraccingCategory>(IssueList.Select(x => x.Category).Distinct().ToList());
            CategoryList.Insert(0, new PraccingCategory() { Category = "<All>", ID=-1 });
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


            SelectedDates = [DateList[0]];
            SelectedFrom = [FromList[0]];
            SelectedTo = [ToList[0]];
            SelectedCategory = [CategoryList[0]];
            SelectedLanguage = LanguageList[0];

            SelectedLUDates = [LastUpdateDateList[0]];
            SelectedLUFrom = [LastUpdateFromList[0]];
            SelectedLUTo = [LastUpdateToList[0]];

            OnPropertyChanged(nameof(SelectedDates));
            OnPropertyChanged(nameof(SelectedFrom));
            OnPropertyChanged(nameof(SelectedTo));
            OnPropertyChanged(nameof(SelectedCategory));
            OnPropertyChanged(nameof(SelectedLUDates));
            OnPropertyChanged(nameof(SelectedLUFrom));
            OnPropertyChanged(nameof(SelectedLUTo));
            OnPropertyChanged(nameof(SelectedLanguage));

        }

        private void GetLastUpdateInfo()
        {
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

            LastUpdateDateList = new ObservableCollection<string>(lastUpdateDates);
            LastUpdateDateList.Insert(0, "<All>");
            LastUpdateFromList = new ObservableCollection<Person>(lastUpdateFromNames);
            LastUpdateFromList.Insert(0, new Person("<All>", -1));
            LastUpdateToList = new ObservableCollection<Person>(lastUpdateToNames);
            LastUpdateToList.Insert(0, new Person("<All>", -1));
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

            if (SelectedDates != null && SelectedDates.Count > 0 && !SelectedDates.Contains("<All>"))
            {
                var selectedDateTimes = SelectedDates.Select(d => DateTime.ParseExact(d, "ddMMMyyyy", null).Date).ToList();
                issues = issues.Where(x => selectedDateTimes.Contains(x.IssueDate.Date));
            }

            if (SelectedFrom != null && SelectedFrom.Count > 0 && !SelectedFrom.Any(p => p.ID == -1))
                issues = issues.Where(x => SelectedFrom.Contains(x.IssueFrom));

            if (SelectedTo != null && SelectedTo.Count > 0 && !  SelectedTo.Any(p => p.ID == -1))
                issues = issues.Where(x => SelectedTo.Contains(x.IssueTo));

            if (SelectedCategory !=null && SelectedCategory.Count > 0 && !SelectedCategory.Any(c => c.Category == "<All>"))
                issues = issues.Where(x => SelectedCategory.Contains(x.Category));


            if (SelectedLUDates.Count() != 0)
            {
                issues = issues.Where(x => (x.Responses.Count() == 0 && SelectedLUDates.Contains(x.IssueDate.ToString("d"))) ||
                    (x.Responses.Count() > 0 && SelectedLUDates.Contains(x.Responses.Last().ResponseDate.Value.ToString("d"))));
            }

            if (SelectedLUFrom.Count() != 0)
            {
                issues = issues.Where(x => (x.Responses.Count() == 0 && SelectedLUFrom.Contains(x.IssueFrom)) ||
                    (x.Responses.Count() > 0 && SelectedLUFrom.Contains(x.Responses.Last().ResponseFrom)));
            }

            if (SelectedLUTo.Count() != 0)
            {
                issues = issues.Where(x => (x.Responses.Count() == 0 && SelectedLUTo.Contains(x.IssueTo)) ||
                    (x.Responses.Count() > 0 && SelectedLUTo.Contains(x.Responses.Last().ResponseTo)));
            }

            return issues.ToList();
        }

        List<string> GetRecipients()
        {
            var to = SelectedTo.FirstOrDefault()?.ID != -1
                ? SelectedTo.Select(p => p.Name)
                : Enumerable.Empty<string>();

            var luTo = SelectedLUTo.FirstOrDefault()?.ID != -1
                ? SelectedLUTo.Select(p => p.Name)
                : Enumerable.Empty<string>();

            return to
                .Concat(luTo)
                .Distinct()
                .ToList();
        }


        [RelayCommand]
        private void OpenReportsFolder()
        {
            System.Diagnostics.Process.Start(@"\\psychfile\psych$\psych-lab-gfong\SMG\SDI\Reports\Praccing");
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
