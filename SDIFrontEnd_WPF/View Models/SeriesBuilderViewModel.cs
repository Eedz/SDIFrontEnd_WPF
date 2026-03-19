using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITCLib;
using MvvmLib.ViewModels;
using RtfPipe.Tokens;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class SeriesBuilderViewModel : WorkspaceViewModel
    {
        private readonly ReferenceDataStore _referenceData;
        private readonly IApiWordingService _wordingService;

        [ObservableProperty]
        private SurveyQuestion? currentQuestion;

        public List<string> FieldList { get; } = new List<string> { string.Empty, "PreP", "PreI", "PreA", "PstI", "PstP", "RespOptions", "NRCodes", 
                                                                            "Content", "Topic", "Domain", "Product" };

        [ObservableProperty]
        private string? selectedField;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedItemText))]
        private object? selectedItem;

        [ObservableProperty]
        private ObservableCollection<object>? listItems;

        public List<VarNameLabel> ContentLabels { get; set; }
        public List<VarNameLabel> TopicLabels { get; set; }
        public List<VarNameLabel> DomainLabels { get; set; }
        public List<VarNameLabel> ProductLabels { get; set; }

        public List<Wording> PrePs { get; set; }
        public List<Wording> PreIs { get; set; }
        public List<Wording> PreAs { get; set; }
        public List<Wording> LitQs { get; set; }
        public List<Wording> PstIs { get; set; }
        public List<Wording> PstPs { get; set; }
        public List<ResponseSet> RespOptions { get; set; }
        public List<ResponseSet> NRCodes { get; set; }

        [ObservableProperty]
        private ObservableCollection<SurveyQuestion> newQuestions;

        public string SelectedItemText
        {
            get
            {
                if (SelectedItem is Wording)
                {
                    return ((Wording)SelectedItem).WordingText;
                }
                else if (SelectedItem is ResponseSet)
                {
                    return ((ResponseSet)SelectedItem).RespList;
                }
                else if (SelectedItem is VariableLabel)
                {
                    return ((VariableLabel)SelectedItem).LabelText;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        [ObservableProperty]
        private int currentPreP;
        [ObservableProperty]
        private int currentPreI;
        [ObservableProperty]
        private int currentPreA;
        [ObservableProperty]
        private int currentLitQ;
        [ObservableProperty]
        private int currentPstI;
        [ObservableProperty]
        private int currentPstP;
        [ObservableProperty]
        private string currentRespOptions;
        [ObservableProperty]
        private string currentNRCodes;

        public string CurrentQuestionText => CurrentQuestion == null ? string.Empty : CurrentQuestion.GetQuestionTextHTML(true);

        #region Constructor
        public SeriesBuilderViewModel(ReferenceDataStore referenceData, IApiWordingService wordingService)
        {
            base.DisplayName = "Series Builder";

            _referenceData = referenceData;
            _wordingService = wordingService;



            _ = Load();

            NewQuestions = new ObservableCollection<SurveyQuestion>();
        }
        #endregion

        async Task Load()
        {
            ContentLabels = _referenceData.ContentLabels.ToList();
            TopicLabels = _referenceData.TopicLabels.ToList();
            DomainLabels = _referenceData.DomainLabels.ToList();
            ProductLabels = _referenceData.ProductLabels.ToList();

            PrePs = await _wordingService.GetAllPreP();
            PreIs = await _wordingService.GetAllPreI();
            PreAs = await _wordingService.GetAllPreA();
            LitQs = await _wordingService.GetAllLitQ();
            PstIs = await _wordingService.GetAllPstI();
            PstPs = await _wordingService.GetAllPstP();
            RespOptions = await _wordingService.GetAllRespOptions();
            NRCodes = await _wordingService.GetAllNonResponses();
        }

        partial void OnCurrentQuestionChanged(SurveyQuestion? value)
        {
            if (value == null)
            {

                CurrentPreP = 0;
                CurrentPreI = 0;
                CurrentPreA = 0;
                CurrentLitQ = 0;
                CurrentPstI = 0;
                CurrentPstP = 0;
                CurrentRespOptions = string.Empty;
                CurrentNRCodes = string.Empty;
            }
            else
            {
                // update wordings and response sets
                CurrentPreP = value.PrePW.WordID;
                CurrentPreI = value.PreIW.WordID;
                CurrentPreA = value.PreAW.WordID;
                CurrentLitQ = value.LitQW.WordID;
                CurrentPstI = value.PstIW.WordID;
                CurrentPstP = value.PstPW.WordID;
                CurrentRespOptions = value.RespOptionsS.RespSetName;
                CurrentNRCodes = value.NRCodesS.RespSetName;
            }
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnSelectedFieldChanged(string? value)
        {
            switch (SelectedField)
            {
                case "PreP":
                    ListItems = new ObservableCollection<object>(PrePs.Cast<object>());
                    break;
                case "PreI":
                    ListItems = new ObservableCollection<object>(PreIs.Cast<object>());
                    break;
                case "PreA":
                    ListItems = new ObservableCollection<object>(PreAs.Cast<object>());
                    break;
                case "LitQ":
                    ListItems = new ObservableCollection<object>(LitQs.Cast<object>());
                    break;
                case "PstI":
                    ListItems = new ObservableCollection<object>(PstIs.Cast<object>());
                    break;
                case "PstP":
                    ListItems = new ObservableCollection<object>(PstPs.Cast<object>());
                    break;
                case "RespOptions":
                    ListItems = new ObservableCollection<object>(RespOptions.Cast<object>());
                    break;
                case "NRCodes":
                    ListItems = new ObservableCollection<object>(NRCodes.Cast<object>());
                    break;
                case "Content":
                    ListItems = new ObservableCollection<object>(ContentLabels.Cast<object>());
                    break;
                case "Topic":
                    ListItems = new ObservableCollection<object>(TopicLabels.Cast<object>());
                    break;
                case "Domain":
                    ListItems = new ObservableCollection<object>(DomainLabels.Cast<object>());
                    break;
                case "Product":
                    ListItems = new ObservableCollection<object>(ProductLabels.Cast<object>());
                    break;
                default:
                    ListItems = new ObservableCollection<object>();
                    break;
            }
            OnPropertyChanged(nameof(ListItems));
        }

   
        partial void OnCurrentPrePChanged(int oldValue, int newValue)
        {
            if (!PrePs.Any(x => x.WordID == newValue)) { 
                CurrentPreP = oldValue;
                return;
            }
            CurrentQuestion.PrePW = PrePs.First(x => x.WordID == newValue);
            OnPropertyChanged(nameof(CurrentQuestionText));
        }
        partial void OnCurrentPreIChanged(int oldValue, int newValue)
        {
            if (!PreIs.Any(x => x.WordID == newValue))
            {
                CurrentPreI = oldValue;
                return;
            }
            CurrentQuestion.PreIW = PreIs.First(x => x.WordID == newValue);
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnCurrentPreAChanged(int oldValue, int newValue)
        {
            if (!PreAs.Any(x => x.WordID == newValue))
            {
                CurrentPreA = oldValue;
                return;
            }
            CurrentQuestion.PreAW = PreAs.First(x => x.WordID == newValue);
            OnPropertyChanged(nameof(CurrentQuestionText));
        }
        partial void OnCurrentLitQChanged(int oldValue, int newValue)
        {
            if (!LitQs.Any(x => x.WordID == newValue))
            {
                CurrentLitQ = oldValue;
                return;
            }
            CurrentQuestion.LitQW = LitQs.First(x => x.WordID == newValue);
            OnPropertyChanged(nameof(CurrentQuestionText));
        }
        partial void OnCurrentPstIChanged(int oldValue, int newValue)
        {
            if (!PstIs.Any(x => x.WordID == newValue))
            {
                CurrentPstI = oldValue;
                return;
            }
            CurrentQuestion.PstIW = PstIs.First(x => x.WordID == newValue);
            OnPropertyChanged(nameof(CurrentQuestionText));
        }
        partial void OnCurrentPstPChanged(int oldValue, int newValue)
        {
            if (!PstPs.Any(x => x.WordID == newValue))
            {
                CurrentPstP = oldValue;
                return;
            }
            CurrentQuestion.PstPW = PstPs.First(x => x.WordID == newValue);
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnCurrentRespOptionsChanged(string? oldValue, string newValue)
        {
            if (!RespOptions.Any(x => x.RespSetName == newValue))
            {
                CurrentRespOptions = oldValue;
                return;
            }
            CurrentQuestion.RespOptionsS = RespOptions.First(x => x.RespSetName == newValue);
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnCurrentNRCodesChanged(string? oldValue, string newValue)
        {
            if (!NRCodes.Any(x => x.RespSetName == newValue))
            {
                CurrentNRCodes = oldValue;
                return;
            }
            CurrentQuestion.NRCodesS = NRCodes.First(x => x.RespSetName == newValue);
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        [RelayCommand]
        private void AddQuestion(string varname)
        {
            SurveyQuestion newQuestion = new SurveyQuestion
            {
                Qnum = $"000{ToLetterCode(NewQuestions.Count)}",
                VarName = new VariableName(varname),
                LitQW = new Wording(0, "LitQ", "new member"),
            };
            NewQuestions.Add(newQuestion);
            CurrentQuestion = newQuestion;
        }

        [RelayCommand]
        private void RemoveQuestion(SurveyQuestion question)
        {
            NewQuestions.Remove(question);
            CurrentQuestion = null;
        }

        [RelayCommand]
        private void Save()
        {
            CloseCommand.Execute(true);
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseCommand.Execute(false);
        }

        [RelayCommand]
        private void ApplyWording()
        {
            if (SelectedItem is Wording)
            {

                UpdateWording((Wording)SelectedItem);
            }
            else if (SelectedItem is ResponseSet)
            {
                UpdateResponseSet((ResponseSet)SelectedItem);
            }
            else if (SelectedItem is VariableLabel)
            {
                UpdateLabel((VariableLabel)SelectedItem);
            }
            else
            {

            }
        }

        private void UpdateWording(Wording wording)
        {
            switch (wording.Type)
            {
                case WordingType.PreP:
                    UpdatePreP(wording);
                    break;
                case WordingType.PreI:
                    UpdatePreI(wording);
                    break;
                case WordingType.PreA:
                    UpdatePreA(wording);
                    break;

                case WordingType.LitQ:
                    UpdateLitQ(wording);
                    break;
                case WordingType.PstI:
                    UpdatePstI(wording);
                    break;
                case WordingType.PstP:
                    UpdatePstP(wording);
                    break;
            }
        }

        private void UpdatePreP(Wording prep)
        {
            foreach (SurveyQuestion question in NewQuestions)
            {
                question.PrePW = prep;
            }
        }

        private void UpdatePreI(Wording prei)
        {
            foreach (SurveyQuestion question in NewQuestions)
            {
                question.PreIW = prei;
            }
        }

        private void UpdatePreA(Wording prea)
        {
            foreach (SurveyQuestion question in NewQuestions)
            {
                question.PreAW = prea;
            }
        }

        private void UpdateLitQ(Wording litq)
        {
            foreach (SurveyQuestion question in NewQuestions)
            {
                question.LitQW = litq;
            }
        }

        private void UpdatePstI(Wording psti)
        {
            foreach (SurveyQuestion question in NewQuestions)
            {
                question.PstIW = psti;
            }
        }

        private void UpdatePstP(Wording pstp)
        {
            foreach (SurveyQuestion question in NewQuestions)
            {
                question.PstPW = pstp;
            }
        }

        private void UpdateResponseSet(ResponseSet respSet)
        {
            switch (respSet.Type)
            {
                case ResponseType.RespOptions:
                    UpdateRespOptions(respSet);
                    break;
                case ResponseType.NRCodes:
                    UpdateNRCodes(respSet);
                    break;
            }
        }

        private void UpdateRespOptions(ResponseSet respSet)
        {
            foreach (SurveyQuestion question in NewQuestions)
            {
                question.RespOptionsS = respSet;
            }
        }

        private void UpdateNRCodes(ResponseSet respSet)
        {
            foreach (SurveyQuestion question in NewQuestions)
            {
                question.NRCodesS = respSet;
            }
        }

        private void UpdateLabel(VariableLabel label)
        {
            switch (SelectedField)
            {
                case "Topic":
                    UpdateTopicLabel(label);
                    break;
                case "Domain":
                    UpdateDomainLabel(label);
                    break;
                case "Product":
                    UpdateProductLabel(label);
                    break;
            }
        }

        private void UpdateVarLabel(string label)
        {
            foreach(SurveyQuestion question in NewQuestions)
            {
                question.VarName.VarLabel = label;
            }
        }

        private void UpdateContentLabel(VariableLabel label)
        {
            foreach (SurveyQuestion question in NewQuestions)
            {
                question.VarName.Content = new ContentLabel(label.ID, label.LabelText);
            }
        }

        private void UpdateTopicLabel(VariableLabel label)
        {
            foreach (SurveyQuestion question in NewQuestions)
            {
                question.VarName.Topic = new TopicLabel(label.ID, label.LabelText);
            }
        }

        private void UpdateDomainLabel(VariableLabel label)
        {
            foreach (SurveyQuestion question in NewQuestions)
            {
                question.VarName.Domain = new DomainLabel(label.ID, label.LabelText);
            }
        }

        private void UpdateProductLabel(VariableLabel label)
        {
            foreach (SurveyQuestion question in NewQuestions)
            {
                question.VarName.Product = new ProductLabel(label.ID, label.LabelText);
            }
        }

        private string ToLetterCode(int number)
        {
            string result = string.Empty;

            //number--; // shift to 0-based
            result = (char)('a' + (number % 26)) + "";
               
            
            return result;
        }
    }
}
