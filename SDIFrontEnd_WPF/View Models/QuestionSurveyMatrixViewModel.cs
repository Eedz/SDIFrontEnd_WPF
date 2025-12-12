using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITC_Services;
using ITCLib;
using ITCReportLib;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class VariableGridOptions :ObservableObject
    {
        [ObservableProperty]
        private bool respOptions;
        [ObservableProperty]
        private bool nRCodes;
        [ObservableProperty]
        private bool fmtNames;
        [ObservableProperty]
        private bool varName;
        [ObservableProperty]
        private bool topic;
        [ObservableProperty]
        private bool content;
        [ObservableProperty]
        private bool varLabel;
    }

    public partial class QuestionSurveyMatrixViewModel : WorkspaceViewModel
    {
        private readonly ISurveyService _surveyService;
        private readonly IMatrixService _matrixService;

        public VariableGridOptions Options;

        [ObservableProperty]
        private Survey? selectedSurvey;

        public ObservableCollection<Survey> SelectedSurveys { get; } = new();

        private readonly List<SurveyQuestion> _availableQuestions = new();
        public IReadOnlyList<SurveyQuestion> AvailableQuestions => _availableQuestions;

        private const int VisibleRowCount = 20;
        private const int VisibleColumnCount = 10;

        private int _rowOffset;
        public int RowOffset
        {
            get => _rowOffset;
            set
            {
                _rowOffset = Math.Clamp(value, 0, Math.Max(0, AvailableQuestions.Count - VisibleRowCount));
                RefreshWindow();
            }
        }

        private int _columnOffset;
        public int ColumnOffset
        {
            get => _columnOffset;
            set
            {
                _columnOffset = Math.Clamp(value, 0, Math.Max(0, SelectedSurveys.Count - VisibleColumnCount));
                RefreshWindow();
            }
        }

        public List<SurveyQuestion> AllQuestions { get; }
        public List<Survey> AllSurveys { get; }

        public ObservableCollection<SurveyQuestion> VisibleQuestions { get; } = new();
        public ObservableCollection<Survey> VisibleSurveys { get; } = new();
        public ObservableCollection<IReadOnlyList<string>> VisibleGrid { get; } = new();

        // Prevent reloading questions multiple times
        private readonly Dictionary<int, Task<List<SurveyQuestion>>> _loadingTasks = new();

        [ObservableProperty]
        private bool excludeHeadings;

        [ObservableProperty]
        private bool excludeBI;

        [ObservableProperty]
        private bool stdOnly;

        [ObservableProperty]
        private bool excludeScreeners;


        public QuestionSurveyMatrixViewModel(ISurveyService surveyService, IMatrixService matrixService)
        {
            _surveyService = surveyService;
            _matrixService = matrixService;
            Options = new VariableGridOptions();
            AllQuestions = new List<SurveyQuestion>();
            AllSurveys = surveyService.GetAllSurveys();

            SelectedSurveys.CollectionChanged += SelectedSurveys_CollectionChanged;
            RefreshWindow();
            
        }

        [RelayCommand]
        private async Task AddSurveyAsync()
        {
            if (SelectedSurvey == null || SelectedSurveys.Contains(SelectedSurvey))
                return;

            var survey = SelectedSurvey;
            SelectedSurveys.Add(survey);

            var questions = await _matrixService.LoadSurveyQuestionsAsync(survey.SID);

            foreach (var q in questions)
                survey.Questions.Add(q);

            RebuildAvailableQuestions();
        }

        [RelayCommand]
        private void RemoveSurvey(Survey survey)
        {
            SelectedSurveys.Remove(survey);
            RebuildAvailableQuestions();
        }

        [RelayCommand]
        private void ScrollRows(int delta)
        {
            _rowOffset = Math.Clamp(
                _rowOffset + delta,
                0,
                Math.Max(0, AllQuestions.Count - VisibleRowCount));

            RefreshWindow();
        }

        [RelayCommand]
        private void ScrollColumns(int delta)
        {
            _columnOffset = Math.Clamp(
                _columnOffset + delta,
                0,
                Math.Max(0, AllSurveys.Count - VisibleColumnCount));

            RefreshWindow();
        }


        [RelayCommand]
        private void Export()
        {
            DataTable data = new DataTable();

            data.Columns.Add("VarName", typeof(string));
            data.Columns.Add("VarLabel", typeof(string));

            foreach (Survey s in SelectedSurveys)
            {
                data.Columns.Add(s.SurveyCode, typeof(string));
            }

            var totalRow = data.NewRow();
            totalRow["VarName"] = "Total";

            foreach (Survey s in SelectedSurveys)
            {
                totalRow[s.SurveyCode] = s.Questions.Count();
            }
            data.Rows.Add(totalRow);

            foreach (SurveyQuestion q in AvailableQuestions)
            {
                var row = data.NewRow();
                row["VarName"] = q.VarName.RefVarName;
                row["VarLabel"] = q.VarName.VarLabel;
                foreach (Survey s in SelectedSurveys)
                {
                    var sq = s.Questions.FirstOrDefault(x => x.VarName.RefVarName == q.VarName.RefVarName);
                    if (sq != null)
                    {
                        row[s.SurveyCode] = sq.Qnum;
                    }
                    else
                    {
                        row[s.SurveyCode] = string.Empty;
                    }
                }
                data.Rows.Add(row);
            }


            DataTableReport rpt = new DataTableReport(data, "Variable List");
            
            rpt.CreateReport();
            rpt.OutputReport();
        }

        [RelayCommand]
        private void ToggleHeadings()
        {
            RefreshWindow();
        }

        private void SelectedSurveys_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)        
        {
            // Reset window so user doesn’t land on invalid offsets
            RowOffset = 0;
            ColumnOffset = 0;
        }

        private void RebuildAvailableQuestions()
        {
            var merged = _matrixService.MergeQuestions(SelectedSurveys);
            var sorted = _matrixService.SortQuestions(merged);

            _availableQuestions.Clear();
            _availableQuestions.AddRange(sorted);

            OnPropertyChanged(nameof(AvailableQuestions));
            RefreshWindow();
        }

        private void RefreshWindow()
        {
            VisibleQuestions.Clear();
            VisibleSurveys.Clear();
            VisibleGrid.Clear();

            var questions = AvailableQuestions
                .Skip(RowOffset)
                .Take(VisibleRowCount)
                .ToList();

            var surveys = SelectedSurveys
                .Skip(ColumnOffset)
                .Take(VisibleColumnCount)
                .ToList();

            foreach (var q in questions)
            {
                if (ExcludeHeadings && (q.IsHeading() || q.IsSubHeading())) continue;
                if (ExcludeBI && q.ScriptOnly && q.VarName.RefVarName.StartsWith("BI")) continue;
                if (StdOnly && !q.VarName.StandardForm) continue;
                VisibleQuestions.Add(q);
            }
            foreach (var s in surveys) VisibleSurveys.Add(s);

            foreach (var q in questions)
            {
                if (ExcludeHeadings && (q.IsHeading() || q.IsSubHeading())) continue;
                if (ExcludeBI && q.ScriptOnly && q.VarName.RefVarName.StartsWith("BI")) continue;
                if (StdOnly && !q.VarName.StandardForm) continue;

                var row = new List<string>();

                foreach (var s in surveys)
                {
                    row.Add(s.Questions.Any(x => x.VarName.RefVarName == q.VarName.RefVarName)
                            ? s.QuestionByRefVar(q.VarName.RefVarName).Qnum
                            : string.Empty);
                }

                VisibleGrid.Add(row);
            }
        }

        

    }
}