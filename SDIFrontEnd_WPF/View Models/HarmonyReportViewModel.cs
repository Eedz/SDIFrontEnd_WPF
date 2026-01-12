using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


using ITC_Services;
using ITCLib;
using ITCReportLib;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace SDIFrontEnd_WPF.ViewModels
{
    public enum HarmonyReportDisplay { Surveys, Projects };
    public partial class HarmonyReportViewModel : WorkspaceViewModel
    {
        private readonly ISurveyService _surveyService;
        private readonly IVarNameService _varNameService;
        
        public HarmonyReportDisplay DisplayOption { get; set; } = HarmonyReportDisplay.Surveys;

        public List<VariableName> VarNameList { get; set; } = new List<VariableName>();
        public ObservableCollection<VariableName> SelectedVars { get; set; } = new ObservableCollection<VariableName>();
        [ObservableProperty]
        private VariableName? selectedVar;
        [ObservableProperty]
        private VariableName? selectedListVar;
        public List<string> PrefixList { get; set; } = new List<string>();
        public ObservableCollection<string> SelectedPrefixes { get; set; } = new ObservableCollection<string>();

        // Surveys, Waves, Studies
        public List<Survey> SurveyList { get; set; } = new List<Survey>();
      
        public ObservableCollection<Survey> SelectedSurveys { get; set; } = new ObservableCollection<Survey>();
        [ObservableProperty]
        private Survey? selectedSurvey;
        public List<StudyWave> WaveList { get; set; } = new List<StudyWave>();
    
        public List<Study> StudyList { get; set; } = new List<Study>();

        public ObservableCollection<Study> SelectedStudies { get; } = new();
        public ObservableCollection<StudyWave> SelectedWaves { get; } = new();

        public ICollectionView FilteredStudyWaves { get; }
        public ICollectionView FilteredSurveys { get; }


        public List<string> GroupOnList { get; set; } = new List<string>()
        {
            "PreP",
            "PreI",
            "PreA",
            "LitQ",
            "RespOptions",
            "NRCodes",
            "PstI",
            "PstP",
            "VarLabel",
            "Domain",
            "Topic",
            "Content",
            "Product"
        };

        public ObservableCollection<string> SelectedGroupOnList { get; set; } = new ObservableCollection<string>();

        
        

        
        [ObservableProperty]
        private bool showFieldwork = false;
        [ObservableProperty]
        private bool multipleWordingsOnly = false;
        [ObservableProperty]
        private bool showGroupOn = false;
        [ObservableProperty]
        private bool separateLabels = false;
        [ObservableProperty]
        private string customFileName= string.Empty;

        [ObservableProperty]
        private string searchText = string.Empty;

        public HarmonyReportViewModel(ISurveyService surveyService, IVarNameService varNameService)
        {
            DisplayName = "Harmony Report";

            _surveyService = surveyService;
            _varNameService = varNameService;

            var allVars = _varNameService.GetAllVarNames();
            VarNameList = allVars.GroupBy(x=>x.RefVarName).Select(x=>x.FirstOrDefault()).OrderBy(v => v.VarName).ToList();
            PrefixList = VarNameList.Select(v => v.VarName.Substring(0, 2)).Distinct().OrderBy(p => p).ToList();
            SurveyList = _surveyService.GetAllSurveys().OrderBy(s => s.SurveyCode).ToList();
            SurveyList.Insert(0, new Survey() { SID = -1, SurveyCode = "<All>"});
            WaveList = _surveyService.GetAllWaves().OrderBy(w => w.ISO_Code).ThenBy(w => w.Wave).ToList();
            WaveList.Insert (0, new StudyWave() { ID = -1, ISO_Code = "<All>"});
            StudyList = _surveyService.GetAllStudies().OrderBy(s => s.StudyName).ToList();
            StudyList.Insert(0, new Study() { ID = -1, StudyName = "<All>" });

            SelectedGroupOnList.Add("PreA");
            SelectedGroupOnList.Add("LitQ");
            SelectedGroupOnList.Add("RespOptions");
            
            // surveys tab lists
            FilteredStudyWaves = CollectionViewSource.GetDefaultView(WaveList);
            FilteredStudyWaves.Filter = FilterWave;

            SelectedStudies.CollectionChanged += (_, __) => FilteredStudyWaves.Refresh();

            FilteredSurveys = CollectionViewSource.GetDefaultView(SurveyList);
            FilteredSurveys.Filter = FilterSurvey;

            SelectedWaves.CollectionChanged += (_, __) => FilteredSurveys.Refresh();

            SelectedStudies.Add(StudyList[0]);
            SelectedWaves.Add(WaveList[0]);
            SelectedSurveys.Add(SurveyList[0]);

            SelectedStudies.CollectionChanged += SelectedStudies_CollectionChanged;
            SelectedWaves.CollectionChanged += SelectedWaves_CollectionChanged;
            SelectedSurveys.CollectionChanged += SelectedSurveys_CollectionChanged;
        }

        private void SelectedSurveys_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Schedule the selection fix after the event finishes
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                SelectedSurveys.CollectionChanged-= SelectedSurveys_CollectionChanged;
                if (e.NewItems != null)
                {
                    List<Survey> added = e.NewItems.Cast<Survey>().ToList();
                    if (added.Any(x => x.SID == -1))
                    {
                        SelectedSurveys.Clear();
                        SelectedSurveys.Add(SurveyList[0]);
                        return;
                    }

                    else
                    {
                        if (SelectedSurveys.Contains(SurveyList[0]))
                            SelectedSurveys.Remove(SurveyList[0]);
                    }
                }
                SelectedSurveys.CollectionChanged += SelectedSurveys_CollectionChanged;
                SelectedSurveys = new ObservableCollection<Survey>(SelectedSurveys);
                OnPropertyChanged(nameof(SelectedSurveys));
            }));

            //if (e.NewItems != null)
            //{
            //    List<Survey> added = e.NewItems.Cast<Survey>().ToList();
            //    if (added.Any(x => x.SID == -1))
            //    {
            //        SelectedSurveys.Clear();
            //        SelectedSurveys.Add(SurveyList[0]);
            //        return;
            //    }

            //    else
            //    {
            //        if (SelectedSurveys.Contains(SurveyList[0]))
            //            SelectedSurveys.Remove(SurveyList[0]);
            //    }
            //}
        }

        private void SelectedWaves_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void SelectedStudies_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
        }

        private bool FilterWave(object item)
        {
            if (SelectedStudies.Count == 0)
                return true;

            if (SelectedStudies.Any(x => x.ID == -1))
                return true;

            var wave = item as StudyWave;
            return SelectedStudies.Any(x => x.ID == wave.StudyID) || wave.ID == -1;
        }

        private bool FilterSurvey(object item)
        {
            if (SelectedWaves.Count == 0)
                return true;

            if (SelectedWaves.Any(x => x.ID == -1))
                return true;

            var survey = item as Survey;
            return SelectedWaves.Any(x=>x.ID == survey.WaveID) || survey.SID == -1;
        }

        [RelayCommand]
        private void GenerateReport(string type)
        {
            // Implementation for generating the Harmony report goes here.
            if (type == "single")
            {
                HarmonyReport report = new HarmonyReport();
                // TODO get var filter
                DataTable results = GetHarmonyResults(SelectedVars.ToList());

                report.OpenFinalReport = true;
                report.ReportTable = results;
                report.CreateReport();
                report.OutputReport();
            }
            else if (type == "multi")
            {
            }
        }

        Dictionary<string, Func<SurveyQuestion, object?>> GroupColumns =
            new Dictionary<string, Func<SurveyQuestion, object?>>
            {
                ["Domain"] = q => q.VarName.Domain,
                ["Topic"] = q => q.VarName.Topic,
                ["Content"] = q => q.VarName.Content,
                ["Product"] = q => q.VarName.Product,
                ["RefVarName"] = q => q.VarName.RefVarName,
                ["PreP"] = q => q.PrePW,
                ["PreI"] = q => q.PreIW,
                ["PreA"] = q => q.PreAW,
                ["LitQ"] = q => q.LitQW,
                ["PstI"] = q => q.PstIW,
                ["PstP"] = q => q.PstPW,
                ["RespOptions"] = q => q.RespOptionsS,
                ["NRCodes"] = q => q.NRCodesS,
                ["VarLabel"] = q => q.VarName.VarLabel,
                // … add the rest of your 13 properties
            };

        private DataTable GetHarmonyResults(List<VariableName> vars)
        {

            List<SurveyQuestion> questions = new List<SurveyQuestion>();

            foreach (var v in vars)
            {
                var qs = _surveyService.FindQuestionsByRefVarName(v.RefVarName);

                // <All> survey selected
                if (SelectedSurveys.Count == 1 && SelectedSurveys[0].SID == -1)
                    questions.AddRange(qs);
                else
                {

                    questions.AddRange(qs.Where(q => SelectedSurveys.Any(s => s.SurveyCode == q.SurveyCode)).ToList());
                }
                
            }

            var selectedSelectors = SelectedGroupOnList.Select(name => GroupColumns[name]).ToList();

            var result =
                questions.GroupBy(
                    q => selectedSelectors.Select(s => s(q)).ToList(),
                    new ListComparer<object?>()
                );

            // create the results table
            DataTable results = BuildResultTable(); 

            bool showProjects = DisplayOption == HarmonyReportDisplay.Projects;

            bool hasLabels = SelectedGroupOnList.Contains("VarLabel") ||
                                    SelectedGroupOnList.Contains("Content") ||
                                    SelectedGroupOnList.Contains("Topic") ||
                                    SelectedGroupOnList.Contains("Domain") ||
                                    SelectedGroupOnList.Contains("Product");

            // populate the results table
            foreach (var g in result)
            {
                var row = results.NewRow();

                row["RefVarName"] = ((SurveyQuestion)g.FirstOrDefault()).VarName.RefVarName;
                row["Question"] = GetQuestionText((SurveyQuestion)g.FirstOrDefault());
                if (showProjects)
                {
                    row["Surveys"] = GetProjectData(g);
                }
                else
                    row["Surveys"] = GetSurveyData(g);

                if (ShowGroupOn)
                    row["Group On"] = string.Join("<br>", g.Key);

                if (hasLabels)
                    if (SeparateLabels)
                    {
                        if (SelectedGroupOnList.Contains("VarLabel")) row["VarLabel"] = g.FirstOrDefault().VarName.VarLabel;
                        if (SelectedGroupOnList.Contains("Content")) row["Content"] = g.FirstOrDefault().VarName.Content.LabelText;
                        if (SelectedGroupOnList.Contains("Topic")) row["Topic"] = g.FirstOrDefault().VarName.Topic.LabelText;
                        if (SelectedGroupOnList.Contains("Domain")) row["Domain"] = g.FirstOrDefault().VarName.Domain.LabelText;
                        if (SelectedGroupOnList.Contains("Product")) row["Product"] = g.FirstOrDefault().VarName.Product.LabelText;
                    }
                    else
                    {
                        row["Labels"] = GetLabelData(g.FirstOrDefault());
                    }

                results.Rows.Add(row);
            }

            


            //if (multipleWordingsOnly)
            //{
            //    DataTable multiples = (from r in results.AsEnumerable()
            //                           where (
            //                               from c in results.AsEnumerable()
            //                               group c by c.Field<string>("refVarName") into grp
            //                               where grp.Count() > 1
            //                               select grp.Key
            //                           ).Contains(r.Field<string>("refVarName"))
            //                           select r).CopyToDataTable();

            //    results = multiples;
            //}

            if (ShowFieldwork)
            {
                PopulateFieldworkData(results);
            }

            return results;
        }

        /// <summary>
        /// Adds columns to the result table
        /// </summary>
        /// <returns></returns>
        private DataTable BuildResultTable()
        {
            DataTable results = new DataTable();

            results.Columns.Add("refVarName");
            results.Columns.Add("Question");


            bool hasLabels = SelectedGroupOnList.Contains("VarLabel") ||
                                    SelectedGroupOnList.Contains("Content") ||
                                    SelectedGroupOnList.Contains("Topic") ||
                                    SelectedGroupOnList.Contains("Domain") ||
                                    SelectedGroupOnList.Contains("Product");

            if (!SeparateLabels && hasLabels)

                results.Columns.Add("Labels");
            else if (SeparateLabels && hasLabels)
            {
                if (SelectedGroupOnList.Contains("VarLabel"))
                    results.Columns.Add("VarLabel");
                if (SelectedGroupOnList.Contains("Content"))
                    results.Columns.Add("Content");
                if (SelectedGroupOnList.Contains("Topic"))
                    results.Columns.Add("Topic");
                if (SelectedGroupOnList.Contains("Domain"))
                    results.Columns.Add("Domain");
                if (SelectedGroupOnList.Contains("Product"))
                    results.Columns.Add("Product");
            }

            results.Columns.Add("Surveys");

            if (ShowGroupOn)
                results.Columns.Add("Group On");

            return results;
        }

        private string GetLabelData(SurveyQuestion q)
        {
            StringBuilder labels = new StringBuilder();

            foreach (string c in SelectedGroupOnList)
            {
                switch (c)
                {
                    case "VarLabel":
                        labels.Append("V: " + q.GetFullVarLabel() + "<br>");
                        break;
                    case "Content":
                        labels.Append("C: "+ q.VarName.Content.LabelText + "<br>");
                        break;
                    case "Topic":
                        labels.Append("T: " + q.VarName.Topic.LabelText + "<br>");
                        break;
                    case "Domain":
                        labels.Append("D: " + q.VarName.Domain.LabelText + "<br>");
                        break;
                    case "Product":
                        labels.Append("P: " + q.VarName.Product.LabelText + "<br>");
                        break;
                    
                }
            }

            return labels.ToString().TrimEnd("<br>".ToCharArray());
        }

        /// <summary>
        /// Returns the question text based on the group on options
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        private string GetQuestionText(SurveyQuestion q)
        {
            SurveyQuestion temp = new SurveyQuestion();
            temp.VarName = q.VarName;
            foreach(string c in SelectedGroupOnList)
            {
                switch (c)
                {
                    case "PreP":
                        temp.PrePW = q.PrePW;
                        break;
                    case "PreI":
                        temp.PreIW = q.PreIW;
                        break;
                    case "PreA":
                        temp.PreAW = q.PreAW;
                        break;
                    case "LitQ":
                        temp.LitQW = q.LitQW;
                        break;
                    case "PstI":
                        temp.PstIW = q.PstIW;
                        break;
                    case "PstP":
                        temp.PstPW = q.PstPW;
                        break;
                    case "RespOptions":
                        temp.RespOptionsS = q.RespOptionsS;
                        break;
                    case "NRCodes":
                        temp.NRCodesS = q.NRCodesS;
                        break;
                }
            }

            return temp.GetQuestionTextHTML(true);
        }

        private string GetSurveyData(IGrouping<List<object?>, SurveyQuestion> g)
        {
            if (SelectedSurveys.Count()==1 && SelectedSurveys[0].SID==-1)
            {
                return g.Aggregate("", (acc, q) =>
                {
                    if (string.IsNullOrEmpty(acc))
                        return q.SurveyCode;
                    else
                        return acc + ", " + q.SurveyCode;
                });
            }
            else
            {
                return g.Aggregate("", (acc, q) =>
                {
                    if (SelectedSurveys.Any(x => x.SurveyCode == q.SurveyCode))
                    {
                        if (string.IsNullOrEmpty(acc))
                            return q.SurveyCode;
                        else
                            return acc + ", " + q.SurveyCode;
                    }
                    else
                    {
                        return "";
                    }
                });
            }
        
        }

        private string GetProjectData(IGrouping<List<object?>, SurveyQuestion> g)
        {
            if (SelectedSurveys.Count() == 1 && SelectedSurveys[0].SID == -1)
            {
                
                
                return g.Aggregate("", (acc, q) =>
                {
                    string wavecode = WaveList.FirstOrDefault(x => q.SurveyCode.StartsWith(x.WaveCode)).WaveCode;
                    if (string.IsNullOrEmpty(acc))
                        return wavecode;
                    else
                    {
                        if (!acc.Contains(wavecode))
                            return acc + ", " + wavecode;
                        else return acc;
                    }
                });
            }
            else
            {
                return g.Aggregate("", (acc, q) =>
                {
                    if (SelectedSurveys.Any(x => x.SurveyCode == q.SurveyCode))
                    {
                        string wavecode = WaveList.FirstOrDefault(x => q.SurveyCode.StartsWith(x.WaveCode)).WaveCode;
                        if (string.IsNullOrEmpty(acc))
                            return wavecode;
                        else
                            return acc + ", " + wavecode;
                    }
                    else
                    {
                        return "";
                    }
                });
            }

        }

        private void PopulateFieldworkData(DataTable results)
        {
            foreach (DataRow r in results.Rows)
            {
                List<string> surveyList = ((string)r["Surveys"]).Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();

                for (int i = 0; i < surveyList.Count; i++)
                {
                    StudyWave wave = WaveList.Where(x => x.WaveCode.Equals(surveyList[i])).FirstOrDefault();
                    if (wave == null)
                        continue;
                    string fieldwork = wave.GetFieldworkYear();
                    if (!string.IsNullOrEmpty(fieldwork)) surveyList[i] += " (" + fieldwork + ")";
                }

                r["Surveys"] = string.Join(", ", surveyList);
            }
        }

        [RelayCommand]
        private void OpenReportFolder()
        {
            Process.Start("explorer.exe", @"\\psychfile\psych$\psych-lab-gfong\SMG\SDI\Reports\Harmony");
        }

        [RelayCommand]
        private void AddVarName()
        {
            if (!SelectedVars.Contains(SelectedVar))
            {
                SelectedVars.Add(SelectedVar);
                int position = VarNameList.IndexOf(SelectedVar);
                if (position < VarNameList.Count - 1)
                    SelectedVar = VarNameList[VarNameList.IndexOf(SelectedVar) + 1];
            }
        }

        [RelayCommand]
        private void RemoveVarName()
        {
            if (SelectedListVar == null) return;

            int position = VarNameList.IndexOf(SelectedVar);
            if (position < VarNameList.Count - 1)
                SelectedVar = VarNameList[VarNameList.IndexOf(SelectedVar) + 1];
            SelectedVars.Remove(SelectedListVar);



        }

        [RelayCommand]
        private void SelectRecentSurveys()
        {
            var recentWaves = WaveList.Where(w => w.FieldworkDates != null && w.FieldworkDates.Any(fw => fw.End >= DateTime.Now.AddYears(-5))).ToList();
            foreach(Survey s in SurveyList.Where(x => recentWaves.Any(w => w.ID == x.WaveID)))
                SelectedSurveys.Add(s);
        }

        [RelayCommand]
        private void Reset()
        {
            SelectedVars.Clear();
            SelectedSurveys.Clear();
        }
    }
}
