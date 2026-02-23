using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;
using MvvmLib.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class QuestionHistoryViewModel : ViewModelBase
    {
        private List<ChangedSurveyQuestion> _questionVersions;

        public IEnumerable<ChangedSurveyQuestion> QuestionVersions => _questionVersions;

        [ObservableProperty]
        private ChangedSurveyQuestion? selectedVersion;

        public ObservableCollection<Wording> WordingList { get; set; }
        public ObservableCollection<ResponseSet> ResponseList { get; set; }

        [ObservableProperty]
        private Wording selectedWording;

        [ObservableProperty]
        private ResponseSet selectedResponse;

        public ObservableCollection<AuditWording> WordingHistories { get; set; }

        public string WordingNumWidth = "0";


        public string CurrentQuestionText => SelectedVersion != null ? SelectedVersion.GetQuestionTextHTML() : string.Empty;

        public QuestionHistoryViewModel()
        {
            WordingList = new ObservableCollection<Wording>();
            ResponseList = new ObservableCollection<ResponseSet>();
            DisplayName = "Question History";
        }

        public void Load(IEnumerable<ChangedSurveyQuestion> versions, bool showWordID = false)
        {
            _questionVersions = new List<ChangedSurveyQuestion>();
            _questionVersions = versions.ToList();
            OnPropertyChanged(nameof(QuestionVersions));
            SelectedVersion = _questionVersions.FirstOrDefault();
            OnPropertyChanged(nameof(CurrentQuestionText));
            OnPropertyChanged(nameof(QuestionVersions));
            if (showWordID) WordingNumWidth = "Auto";
                else WordingNumWidth = "0";

            OnPropertyChanged(nameof(WordingNumWidth));
        }

        partial void OnSelectedVersionChanged(ChangedSurveyQuestion? oldValue, ChangedSurveyQuestion? newValue)
        {
            OnPropertyChanged(nameof(CurrentQuestionText));

            if (newValue == null)
            {
                WordingList.Clear();
                ResponseList.Clear();
                return;
            }

            WordingList = new ObservableCollection<Wording>
            {
                newValue.PrePW,
                newValue.PreIW,
                newValue.PreAW,
                newValue.LitQW,
                newValue.PstIW,
                newValue.PstPW,
            };

            ResponseList = new ObservableCollection<ResponseSet>() { SelectedVersion.RespOptionsS, SelectedVersion.NRCodesS };
            OnPropertyChanged(nameof(WordingList));
        }

        partial void OnSelectedResponseChanged(ResponseSet? oldValue, ResponseSet newValue)
        {
            throw new NotImplementedException();
        }


    }
}
