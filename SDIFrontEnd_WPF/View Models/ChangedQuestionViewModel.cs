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
    public partial class ChangedQuestionViewModel : ViewModelBase
    {
        private ChangedSurveyQuestion _questionVersion;
        public ChangedSurveyQuestion QuestionVersion => _questionVersion;

        public ObservableCollection<Wording> WordingList { get; set; }
        public ObservableCollection<ResponseSet> ResponseList { get; set; }

        [ObservableProperty]
        private Wording selectedWording;

        [ObservableProperty]
        private ResponseSet selectedResponse;

        public string WordingNumWidth = "0";

        public string CurrentQuestionText => _questionVersion.GetQuestionTextHTML();

        public ChangedQuestionViewModel()
        {
            WordingList = new ObservableCollection<Wording>();
            ResponseList = new ObservableCollection<ResponseSet>();
            DisplayName = "Question History";
        }

        public void Load(ChangedSurveyQuestion version, bool showWordID = false)
        {
            _questionVersion = version;

            WordingList = new ObservableCollection<Wording>
            {
                version.PrePW,
                version.PreIW,
                version.PreAW,
                version.LitQW,
                version.PstIW,
                version.PstPW,
            };

            ResponseList = new ObservableCollection<ResponseSet>() { version.RespOptionsS, version.NRCodesS };

            OnPropertyChanged(nameof(QuestionVersion));
            OnPropertyChanged(nameof(CurrentQuestionText));
            
            if (showWordID) WordingNumWidth = "Auto";
            else WordingNumWidth = "0";

            OnPropertyChanged(nameof(WordingNumWidth));
        }

        partial void OnSelectedResponseChanged(ResponseSet? oldValue, ResponseSet newValue)
        {
            throw new NotImplementedException();
        }


    }
}
