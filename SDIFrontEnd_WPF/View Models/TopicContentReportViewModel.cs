using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office.CustomUI;
using ITCLib;
using ITCReportLib;
using MvvmLib.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class TopicContentReportViewModel : ViewModelBase
    {
        private readonly IApiSurveyService _surveyService;
        private readonly ReferenceDataStore _refData;
        public ObservableCollection<Survey> SurveyList { get; set; } = new ObservableCollection<Survey>();

        [ObservableProperty]
        private Survey? selectedSurvey;
        [ObservableProperty]
        private Survey? selectedReportSurvey;
        public ObservableCollection<Survey> ReportSurveys { get; set; } = new ObservableCollection<Survey>();


        public SelectableList<VarNameLabel>? ProductList { get; set; }
        public ObservableCollection<VarNameLabel> SelectedProductLabels { get; set; } = new ObservableCollection<VarNameLabel>();

        public SelectableList<string>? FieldNames {  get; set; }

        [ObservableProperty]
        private bool includeProduct;

        private string _reportPath = @"\\psychfile\psych$\psych-lab-gfong\SMG\SDI\Reports";

        public TopicContentReportViewModel(ReferenceDataStore refData, IApiSurveyService service) 
        {
            DisplayName = "Topic Content Report";

            _surveyService = service;
            _refData = refData;
        }

        public async Task Load()
        {
            var surveys = await _surveyService.GetAllAsync();
            SurveyList = new ObservableCollection<Survey>(surveys);
            OnPropertyChanged(nameof(SurveyList));

            var products = _refData.ProductLabels;
            ProductList = new SelectableList<VarNameLabel>(products, true);
            ProductList.AllItem.IsSelected = true;

            FieldNames = new SelectableList<string>(new ObservableCollection<string> { "PreP", "PreI", "PreA", "LitQ", "PstI", "PstP", "RespOptions", "NRCodes" }, true);
            FieldNames.AllItem.IsSelected = true;
            
                
        }

        [RelayCommand]
        private async Task GenerateReport()
        {
            TopicContentReport rprt = new TopicContentReport();

            rprt.FileName = _reportPath;
            rprt.ProductCrosstab = IncludeProduct;
            foreach (ReportSurvey s in ReportSurveys)
            {
                var rs = await PopulateSurvey(s);
                rprt.AddSurvey(rs);
            }

            rprt.GenerateLabelReport();
            rprt.OutputReportTableXML();
        }

        [RelayCommand]
        private void AddSurvey()
        {
            if (SelectedSurvey == null) return;

            if (!ReportSurveys.Any(x => x.SurveyCode == SelectedSurvey.SurveyCode))
            {
                ReportSurveys.Add(new ReportSurvey(SelectedSurvey));

                int position = SurveyList.IndexOf(SelectedSurvey);
                if (position < SurveyList.Count - 1)
                {
                    SelectedSurvey = SurveyList[position + 1];

                }
            }


        }

        [RelayCommand]
        private void RemoveSurvey()
        {
            if (SelectedReportSurvey!= null)
                ReportSurveys.Remove(SelectedReportSurvey);
        }

        [RelayCommand]
        private void OpenFolder()
        {
            Process.Start("explorer.exe", @"\\psychfile\psych$\psych-lab-gfong\SMG\SDI\Reports");
        }

        /// <summary>
        /// For each survey in the report, fill the question list, comments and translations as needed.
        /// </summary>
        private async Task<ReportSurvey> PopulateSurvey(ReportSurvey reportSurvey)
        {

            ReportSurvey rs = new ReportSurvey(reportSurvey);
            rs.Questions.Clear();
            rs.SurveyNotes.Clear();
            rs.VarChanges.Clear();

            // questions
            var questions = await _surveyService.GetSurveyQuestions(rs.SID);

            if (FieldNames.IsAllSelected)
                rs.ContentOptions.StdFieldsChosen = new ObservableCollection<string>(FieldNames.Items.Select(x => x.Value));
            else
                rs.ContentOptions.StdFieldsChosen = new ObservableCollection<string>(FieldNames.SelectedValues);

            if (IncludeProduct && !ProductList.IsAllSelected)
            {
                rs.Questions.AddRange(questions.Where(x => ProductList.SelectedValues.Any(y => y.ID == x.VarName.ProductLabel.ID)).ToList());
            }
            else
            {
                rs.Questions.AddRange(questions);
            }

            return rs;
        }
    }
}