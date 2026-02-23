using ITC_Services;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;
using CommunityToolkit.Mvvm.ComponentModel;
using ITC_Services;
using SDIFrontEnd_WPF.Views;
using System.Collections.ObjectModel;

    


namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class QuestionHistoryManagerViewModel : ViewModelBase
    {
        private readonly ISurveyService _surveyService;
        private readonly IAuditService _auditService;
        private readonly ICommentService _commentService;

        public List<Survey> SurveyList { get; set; }

        [ObservableProperty]
        private Survey? selectedSurvey = null;

        [ObservableProperty]
        private VariableName? selectedVarName = null;

        [ObservableProperty]
        private Wording selectedWording = null;

        [ObservableProperty]
        private ResponseSet selectedResponse = null;

        public List<VariableName> VarNameList { get; set; }

        public QuestionHistoryViewModel QuestionHistoryVM { get; set; } = new QuestionHistoryViewModel() { WordingNumWidth = "0" };

        public ObservableCollection<ChangedQuestionViewModel> QuestionHistoryList { get; set; } = new ObservableCollection<ChangedQuestionViewModel>();

        public ObservableCollection<WordingHistoryViewModel> WordingHistoryVMs { get; set; } = new ObservableCollection<WordingHistoryViewModel>();
        public ObservableCollection<Wording> WordingList => QuestionHistoryVM.WordingList;

        public ObservableCollection<QuestionComment> Comments { get; } = new ObservableCollection<QuestionComment>();

        public QuestionHistoryManagerViewModel(IAuditService auditService, ISurveyService surveyService, ICommentService commentService)
        {
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _surveyService = surveyService ?? throw new ArgumentNullException(nameof(surveyService));
            _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));

            DisplayName = "Question History Manager";

            SurveyList = _auditService.GetAuditSurveys().Select(x=> new Survey(x)).ToList();
            VarNameList = new List<VariableName>();
        }

        partial void OnSelectedSurveyChanged(Survey? oldValue, Survey newValue)
        {
            if (newValue == null) return;

            VarNameList = _auditService.GetAuditVarNames(newValue.SurveyCode)
                .Select(q => new VariableName { VarName = q.VarName })
                .OrderBy(v => v.VarName)
                .ToList();

            OnPropertyChanged(nameof(VarNameList));
        }

        partial void OnSelectedVarNameChanged(VariableName? oldValue, VariableName? newValue)
        {
            if (newValue == null || SelectedSurvey == null) return;

            var qid = _surveyService.GetQuestionID(SelectedSurvey.SurveyCode, newValue.VarName);
            var history = _auditService.GetQuestionHistory(qid);
            foreach (var item in history)
            {
                var vm = new ChangedQuestionViewModel();
                vm.Load(item, showWordID: true);
                QuestionHistoryList.Add(vm);
            }
            QuestionHistoryVM.WordingNumWidth = "0";
            QuestionHistoryVM.Load(history);
            OnPropertyChanged(nameof(WordingList));

            Comments.Clear();
            foreach (var c in _commentService.GetQuestionComments(qid))
                Comments.Add(c);
        }

        partial void OnSelectedWordingChanged(Wording? oldValue, Wording newValue)
        {
            var history = _auditService.GetWordingHistory(newValue.FieldType, newValue.WordID);
            WordingHistoryVMs.Clear();

            foreach (var item in history)
            {
                WordingHistoryVMs.Add(new WordingHistoryViewModel(item));
            }
            OnPropertyChanged(nameof(WordingHistoryVMs));
        }
    }   
}
