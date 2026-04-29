using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class QuestionHistoryManagerViewModel : ViewModelBase
    {
        private readonly IApiSurveyService _surveyService;
        private readonly IApiQuestionService _questionService;
        private readonly IApiAuditService _auditService;
        private readonly IApiCommentService _commentService;

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

        public QuestionHistoryManagerViewModel(IApiAuditService auditService, IApiSurveyService surveyService, IApiQuestionService questionService, IApiCommentService commentService)
        {
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _surveyService = surveyService ?? throw new ArgumentNullException(nameof(surveyService));
            _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
            _questionService = questionService ?? throw new ArgumentNullException(nameof(questionService));
            DisplayName = "Question History Manager";
            
           
        }

        public async Task Load()
        {
            SurveyList = (await _auditService.GetAuditSurveys()).Select(x => new Survey(x)).ToList();
            VarNameList = new List<VariableName>();
        }

        partial void OnSelectedSurveyChanged(Survey? oldValue, Survey newValue)
        {
            if (newValue == null) return;

            _ = LoadVarNames(newValue.SurveyCode);
        }

        async Task LoadVarNames(string survey)
        {
            VarNameList = (await _auditService.GetAuditVarNames(survey))
                .Select(q => new VariableName { VarName = q.VarName })
                .OrderBy(v => v.VarName).ToList();
            OnPropertyChanged(nameof(VarNameList));
        }

        async partial void OnSelectedVarNameChanged(VariableName? oldValue, VariableName? newValue)
        {
            if (newValue == null || SelectedSurvey == null) return;

            //var qid = _surveyService.GetQuestionID(SelectedSurvey.SurveyCode, newValue.VarName);
            var vars = await _questionService.GetQuestionsByVarNameAsync(newValue.VarName);
            var qid = vars.FirstOrDefault(q => q.SurveyCode == SelectedSurvey.SurveyCode)?.ID ?? 0;
            var history = await _auditService.GetQuestionHistory(qid);
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
            var comments = await _commentService.GetQuestionCommentsAsync(qid);
            foreach (var c in comments)
                Comments.Add(c);
        }

        async partial void OnSelectedWordingChanged(Wording? oldValue, Wording newValue)
        {
            var history = await _auditService.GetWordingHistory(newValue.FieldType, newValue.WordID);
            WordingHistoryVMs.Clear();

            foreach (var item in history)
            {
                WordingHistoryVMs.Add(new WordingHistoryViewModel(item));
            }
            OnPropertyChanged(nameof(WordingHistoryVMs));
        }
    }   
}
