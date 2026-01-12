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
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class VariableGridOptions : ObservableObject
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

    public enum QuestionGridDisplayMode
    {
        Qnum,
        QuestionText
    }

    public enum QuestionGridSort
    {
        refVarName,
        Qnum
    }

    public enum MatrixColumnType
    {
        Survey,
        Label,
        Qnum,
        VarName,
    }

    public class MatrixColumn
    {
        public string Key { get; }
        public string Header { get; }
        public MatrixColumnType Type { get; }
        public Survey Survey { get; }
        public Func<SurveyQuestion, string>? ValueProvider { get; }

        public MatrixColumn(string key, string header, MatrixColumnType type, Survey survey, Func<SurveyQuestion, string>? valueProvider = null)
        {
            Key = key;
            Header = header;
            Survey = survey;
            Type = type;
            ValueProvider = valueProvider;
        }
    }

    public partial class QuestionSurveyMatrixViewModel : WorkspaceViewModel
    {
        private readonly ISurveyService _surveyService;
        private readonly IMatrixService _matrixService;

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
                _columnOffset = Math.Clamp(value, 0, Math.Max(0, AllColumns.Count - VisibleColumnCount));
                RefreshWindow();
            }
        }

        public List<SurveyQuestion> AllQuestions { get; }
        public List<Survey> AllSurveys { get; }

        public ObservableCollection<SurveyQuestion> VisibleQuestions { get; } = new();
        public ObservableCollection<Survey> VisibleSurveys { get; } = new();
        public ObservableCollection<MatrixColumnViewModel> AllColumns { get; } = new();
        public ObservableCollection<MatrixColumnViewModel> VisibleColumns { get; } = new();

        public ObservableCollection<IReadOnlyList<string>> VisibleGrid { get; } = new();

        [ObservableProperty]
        private bool excludeHeadings;

        [ObservableProperty]
        private bool excludeBI;

        [ObservableProperty]
        private bool stdOnly;

        [ObservableProperty]
        private bool excludeScreeners;

        private bool HeadingsOnly = false;

        [ObservableProperty]
        private QuestionGridDisplayMode displayMode = QuestionGridDisplayMode.Qnum;

        [ObservableProperty]
        private QuestionGridSort sortMode = QuestionGridSort.refVarName;

        [ObservableProperty]
        private int cellHeight = 30;
        [ObservableProperty]
        private int cellWidth = 150;

        public QuestionSurveyMatrixViewModel(ISurveyService surveyService, IMatrixService matrixService)
        {
            _surveyService = surveyService;
            _matrixService = matrixService;

            AllQuestions = new List<SurveyQuestion>();
            AllSurveys = surveyService.GetAllSurveys();

            SelectedSurveys.CollectionChanged += SelectedSurveys_CollectionChanged;
            RefreshWindow();
            
        }





        private MatrixColumnViewModel CreateBaseSurveyColumn(Survey survey)
        {
            var column = new MatrixColumn(
                key: $"survey:{survey.SID}:base",
                header: survey.SurveyCode,
                type: MatrixColumnType.Survey,
                survey: survey,
                valueProvider: q =>
                    survey.Questions.Any(x => x.VarName.RefVarName == q.VarName.RefVarName)
                        ? DisplayText(q)
                        : string.Empty);

            var vm = new MatrixColumnViewModel(column);

            vm.OnSurveyColumnOptionChanged += OnSurveyColumnOptionChanged;

            return vm;
        }

        private void OnSurveyColumnOptionChanged(MatrixColumnViewModel baseColumnVm, string? propertyName)
        {
            var survey = baseColumnVm.Column.Survey;
            var baseIndex = AllColumns.IndexOf(baseColumnVm);

            if (baseIndex < 0)
                return;

            if (propertyName == "Topic")
                ToggleSurveyColumn(
                    survey,
                    baseIndex,
                    "topic",
                    baseColumnVm.Options.Topic,
                    AddTopicColumn);

            if (propertyName == "Content")
                ToggleSurveyColumn(
                    survey,
                    baseIndex,
                    "content",
                    baseColumnVm.Options.Content,
                    AddContentColumn);

            //if (propertyName == nameof(Options.RespOptions))
            //    ToggleSurveyColumn(
            //        survey,
            //        baseIndex,
            //        "resp",
            //        baseColumnVm.Options.RespOptions,
            //        AddRespOptionsColumn);

            RefreshWindow();
        }

        private void ToggleSurveyColumn(Survey survey, int baseIndex, string suffix, bool enabled, Func<Survey, MatrixColumnViewModel> factory)
        {
            var id = $"survey:{survey.SID}:{suffix}";
            var existing = AllColumns.FirstOrDefault(c => c.Column.Key == id);

            if (enabled)
            {
                if (existing != null)
                    return;

                var column = factory(survey);

                // insert immediately AFTER base survey column
                AllColumns.Insert(baseIndex + 1, column);
            }
            else
            {
                if (existing != null)
                    AllColumns.Remove(existing);
            }
        }



        private MatrixColumnViewModel AddTopicColumn(Survey survey)
        {
            return new MatrixColumnViewModel (new MatrixColumn(
                key: $"survey:{survey.SID}:topic",
                header: $"{survey.SurveyCode} Topic",
                type: MatrixColumnType.Label,
                survey: survey,
                valueProvider: q =>
                {
                    var sq = survey.Questions
                        .FirstOrDefault(x => x.VarName.RefVarName == q.VarName.RefVarName);
                    return sq?.VarName.Topic.LabelText ?? string.Empty;
                }));
        }

        private MatrixColumnViewModel AddContentColumn(Survey survey)
        {
            return new MatrixColumnViewModel(new MatrixColumn(
                key: $"survey:{survey.SID}:content",
                header: $"{survey.SurveyCode} content",
                type: MatrixColumnType.Label,
                survey: survey,
                valueProvider: q =>
                {
                    var sq = survey.Questions
                        .FirstOrDefault(x => x.VarName.RefVarName == q.VarName.RefVarName);
                    return sq?.VarName.Content.LabelText ?? string.Empty;
                }));
        }




        [RelayCommand]
        private async Task AddSurveyAsync()
        {
            if (SelectedSurvey == null || SelectedSurveys.Contains(SelectedSurvey))
                return;

            var survey = SelectedSurvey;
            SelectedSurveys.Add(survey);

            var baseColumnVm = CreateBaseSurveyColumn(survey);
            AllColumns.Add(baseColumnVm);

            var questions = await _matrixService.LoadSurveyQuestionsAsync(survey.SID);

            foreach (var q in questions)
                survey.Questions.Add(q);

            RebuildAvailableQuestions();
        }

        [RelayCommand]
        // TODO if survey column, remove all related columns as well
        private void RemoveColumn(MatrixColumnViewModel columnVm)
        {
            AllColumns.Remove(columnVm);

            // if this is a survey column:
            //  rebuild question list
            //  remove from selected surveys
            //  remove all related columns
            // 
            if (columnVm.Column.Type == MatrixColumnType.Survey)
            {
                RebuildAvailableQuestions();
                SelectedSurveys.Remove(columnVm.Column.Survey);               
                foreach(var c in AllColumns.Where(c => c.Column.Key.StartsWith($"survey:{columnVm.Column.Survey.SID}:")).ToList())
                {
                    AllColumns.Remove(c);
                }
            }
            RefreshWindow();
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


        [RelayCommand(CanExecute = nameof(CanExport))]
        
        private void Export()
        {
            if (SelectedSurveys.Count == 0)
                return;

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

        private bool CanExport()
        {
            return SelectedSurveys.Count > 0 && AvailableQuestions.Count > 0;
        }

        [RelayCommand]
        private void UpdateScreen()
        {
            RebuildAvailableQuestions();
        }

        [RelayCommand]
        private void PresetHeadingReport()
        {
            HeadingsOnly = !HeadingsOnly;
            DisplayMode = QuestionGridDisplayMode.QuestionText;
            SortMode = QuestionGridSort.Qnum;
            RebuildAvailableQuestions();
        }

        [RelayCommand]
        private void PresetOverviewReport()
        {
            HeadingsOnly = false;
            DisplayMode = QuestionGridDisplayMode.QuestionText;
           
            SortMode = QuestionGridSort.Qnum;
            RebuildAvailableQuestions();
           
        }

        private void SelectedSurveys_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)        
        {
            // Reset window so user doesn’t land on invalid offsets
            RowOffset = 0;
            ColumnOffset = 0;
            ExportCommand.NotifyCanExecuteChanged();
        }

        partial void OnDisplayModeChanged(QuestionGridDisplayMode value)
        {
            if (value == QuestionGridDisplayMode.QuestionText)
            {
                CellWidth = 250;
                CellHeight = 100;
            }
            else
            {
                CellWidth = 150;
                CellHeight = 30;
            }
            RefreshWindow();
        }

     

        private void RebuildAvailableQuestions()
        {
            var merged = _matrixService.MergeQuestions(SelectedSurveys);

            List<SurveyQuestion> sorted;
            if (SortMode == QuestionGridSort.Qnum)
                sorted = _matrixService.SortQuestionsQnum(merged).ToList();
            else
                sorted = _matrixService.SortQuestions(merged).ToList();

            // apply filters here

            _availableQuestions.Clear();

            foreach (SurveyQuestion q in sorted)
            {
                if (HeadingsOnly)
                {
                    if (q.IsHeading() || q.IsSubHeading())
                    {
                        _availableQuestions.Add(q);
                    }
                }
                else
                {
                    if (ExcludeHeadings && (q.IsHeading() || q.IsSubHeading())) continue;
                    if (ExcludeBI && q.ScriptOnly && q.VarName.RefVarName.StartsWith("BI")) continue;
                    if (StdOnly && !q.VarName.StandardForm) continue;
                    _availableQuestions.Add(q);
                }
                    
            }

            OnPropertyChanged(nameof(AvailableQuestions));
            RefreshWindow();
        }

        private void RefreshWindow()
        {
            VisibleQuestions.Clear();
            VisibleColumns.Clear();
            VisibleGrid.Clear();

            if (AvailableQuestions.Count == 0 || AllColumns.Count == 0)
                return;

            var questions = AvailableQuestions
                .Skip(RowOffset)
                .Take(VisibleRowCount)
                .ToList();

            var columns = AllColumns
                .Skip(ColumnOffset)
                .Take(VisibleColumnCount)
                .ToList();

            foreach (var q in questions)
                VisibleQuestions.Add(q);

            VisibleColumns.Add(new MatrixColumnViewModel(new MatrixColumn(
                key: "refVarName",
                header: "RefVarName",
                type: MatrixColumnType.VarName,
                survey: null!,
                valueProvider: q => q.VarName.RefVarName)));
            foreach (var c in columns)
            {
                VisibleColumns.Add(c);
            }

            foreach (var q in questions)
            {
                var row = columns.Select(c => c.Column.ValueProvider?.Invoke(q) ?? string.Empty).ToList();
                row.Insert(0,q.GetRefVarName());
                VisibleGrid.Add(row);
            }
        }

        string DisplayText (SurveyQuestion q)
        {
            if (DisplayMode == QuestionGridDisplayMode.QuestionText)
                return q.QuestionText;
            else if (DisplayMode == QuestionGridDisplayMode.Qnum)
                return q.Qnum;
            else
                return q.Qnum;
        }

        protected override void OnDispose()
        {
           
          
            
            

            base.Dispose();
        }

    }
}