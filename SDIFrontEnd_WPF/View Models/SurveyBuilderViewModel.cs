using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.ExtendedProperties;
using ITC_DataAccess_EF;
using ITC_Services;
using ITCLib;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SDIFrontEnd_WPF
{
    // TODO add comment
    // TODO save changes
    // TODO view translation (done)
    // TODO undo invalid respname/nrname
    // TODO toggling series/standalone (done)
    // TODO add/remove images
    // TODO add translation shortcut
    // TODO color rows on status (added/removed/modified)
    public partial class SurveyBuilderViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ISurveyService _surveyService;
        private readonly IReferenceDataService _referenceDataService;
        private readonly IWordingService _wordingService;

        private readonly Survey CurrentSurvey;
        private readonly ObservableCollection<SurveyQuestion> _questionList;
        public ObservableCollection<SurveyQuestion> QuestionList => _questionList;

        public List<TopicLabel> TopicLabels { get; set; }
        public List<ContentLabel> ContentLabels { get; set; }
        public List<DomainLabel> DomainLabels { get; set; }
        public List<ProductLabel> ProductLabels { get; set; }

        public ObservableCollection<SurveyQuestion> Added = new ObservableCollection<SurveyQuestion>();
        public ObservableCollection<SurveyQuestion> Removed = new ObservableCollection<SurveyQuestion>();
        public ObservableCollection<SurveyQuestion> Modified = new ObservableCollection<SurveyQuestion>();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentQuestionText))]
        [NotifyPropertyChangedFor(nameof(SectionCount))]
        private SurveyQuestion? selectedQuestion;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ImageIndex))]
        private SurveyImage? currentImage;

        
        public int? ImageIndex => SelectedQuestion?.Images.IndexOf(CurrentImage) + 1 ?? 0;

        [ObservableProperty]
        private ObservableCollection<SurveyQuestion>? selectedQuestions;

        public string CurrentQuestionText => SelectedQuestion.GetQuestionTextHTML() ?? string.Empty;

        public TranslationViewModel TranslationVM;

        public string SectionCount
        {
            get
            {
                if (SelectedQuestion == null)
                    return string.Empty; CurrentSurvey.GetSectionCount(SelectedQuestion);
                string section = CurrentSurvey.GetSectionName(SelectedQuestion);
                section = string.IsNullOrEmpty(section) ? "Before first heading" : section;
                return $"{section} : {CurrentSurvey.GetSectionCount(SelectedQuestion)} member(s).";
            }
        }

        [ObservableProperty]
        private int prePID;
        [ObservableProperty]
        private int preIID;
        [ObservableProperty]
        private int preAID;
        [ObservableProperty]
        private int litQID;
        [ObservableProperty]
        private int pstIID;
        [ObservableProperty]
        private int pstPID;
        [ObservableProperty]
        private string respName;
        [ObservableProperty]
        private string nRName;

        public SurveyBuilderViewModel(IDialogService dialogService, ISurveyService surveyService, IReferenceDataService referenceData, IWordingService wordingService, Survey survey)
        {
            _dialogService = dialogService;
            _surveyService = surveyService ?? throw new ArgumentNullException(nameof(surveyService), "Survey service cannot be null.");
            _referenceDataService = referenceData ?? throw new ArgumentNullException(nameof(referenceData), "Reference data service cannot be null.");
            _wordingService = wordingService ?? throw new ArgumentNullException(nameof(wordingService), "Wording service cannot be null.");
            if (survey == null) throw new ArgumentNullException(nameof(survey), "Questions cannot be null");

            CurrentSurvey = survey;
            _questionList = new ObservableCollection<SurveyQuestion>(survey.Questions);

            TopicLabels= _referenceDataService.GetTopicLabels() ?? new List<TopicLabel>();
            ContentLabels= _referenceDataService.GetContentLabels() ?? new List<ContentLabel>();
            DomainLabels= _referenceDataService.GetDomainLabels() ?? new List<DomainLabel>();
            ProductLabels= _referenceDataService.GetProductLabels() ?? new List<ProductLabel>();

            SelectedQuestion = _questionList.FirstOrDefault() ?? new SurveyQuestion ("Default", "0000" );
           
        }

        partial void OnSelectedQuestionChanged(SurveyQuestion? value)
        {
            if (value == null)
                return;

            CurrentImage = value?.Images.FirstOrDefault();

            PrePID = SelectedQuestion.PrePW.WordID;
            PreIID = SelectedQuestion.PreIW.WordID;
            PreAID = SelectedQuestion.PreAW.WordID;
            LitQID = SelectedQuestion.LitQW.WordID;
            PstIID = SelectedQuestion.PstIW.WordID;
            PstPID = SelectedQuestion.PstPW.WordID;
            RespName = SelectedQuestion.RespOptionsS.RespSetName;
            NRName = SelectedQuestion.NRCodesS.RespSetName;

            if (TranslationVM == null) return;
                
            TranslationVM.UpdateTranslations(SelectedQuestion);
            OnPropertyChanged(nameof(TranslationVM));
        }

        partial void OnPrePIDChanged(int oldValue, int newValue)
        {
            if (SelectedQuestion?.PrePW.WordID == newValue)
                return;

            Wording found = _wordingService.GetAllPreP().FirstOrDefault(x => x.WordID == newValue);
            if (found != null)
            {
                SelectedQuestion.PrePW = found;
                if (!Modified.Contains(SelectedQuestion)) Modified.Add(SelectedQuestion);
            }
            else
                prePID = oldValue;
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnPreIIDChanged(int oldValue, int newValue)
        {
            if (SelectedQuestion?.PreIW.WordID == newValue)
                return;

            Wording found = _wordingService.GetAllPreI().FirstOrDefault(x => x.WordID == newValue);
            if (found != null)
            {
                SelectedQuestion.PreIW = found;
                if (!Modified.Contains(SelectedQuestion)) Modified.Add(SelectedQuestion);
            }
            else
                preIID = oldValue;
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnPreAIDChanged(int oldValue, int newValue)
        {
            if (SelectedQuestion?.PreAW.WordID == newValue)
                return;

            Wording found = _wordingService.GetAllPreA().FirstOrDefault(x => x.WordID == newValue);
            if (found != null)
            {
                SelectedQuestion.PreAW = found;
                if (!Modified.Contains(SelectedQuestion)) Modified.Add(SelectedQuestion);
            }
            else
                preAID = oldValue;
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnLitQIDChanged(int oldValue, int newValue)
        {
            if (SelectedQuestion?.LitQW.WordID == newValue)
                return;
            Wording found = _wordingService.GetAllLitQ().FirstOrDefault(x => x.WordID == newValue);
            if (found != null)
            {
                SelectedQuestion.LitQW = found;
                if (!Modified.Contains(SelectedQuestion)) Modified.Add(SelectedQuestion);
            }
            else
                litQID = oldValue;
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnPstIIDChanged(int oldValue, int newValue)
        {
            if (SelectedQuestion?.PstIW.WordID == newValue)
                return;
            Wording found = _wordingService.GetAllPstI().FirstOrDefault(x => x.WordID == newValue);
            if (found != null)
            {
                SelectedQuestion.PstIW = found;
                if (!Modified.Contains(SelectedQuestion)) Modified.Add(SelectedQuestion);
            }
            else
                pstIID = oldValue;
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnPstPIDChanged(int oldValue, int newValue)
        {
            if (SelectedQuestion?.PstPW.WordID == newValue)
                return;
            Wording? found = _wordingService.GetAllPstP().FirstOrDefault(x => x.WordID == newValue);
            if (found != null)
            {
                SelectedQuestion.PstPW = found;
                if (!Modified.Contains(SelectedQuestion)) Modified.Add(SelectedQuestion);
            }
            else
                pstPID = oldValue;
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnRespNameChanged(string value)
        {
            if (SelectedQuestion?.RespOptionsS.RespSetName == value)
                return;
            ResponseSet found = _wordingService.GetAllResponseSets().FirstOrDefault(x => x.RespSetName == value);
            if (found != null)
            {
                SelectedQuestion.RespOptionsS = found;
                if (!Modified.Contains(SelectedQuestion)) Modified.Add(SelectedQuestion);
            }
            else
            {

            }
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnNRNameChanged(string value)
        {
            if (SelectedQuestion?.NRCodesS.RespSetName == value)
                return;
            ResponseSet found = _wordingService.GetAllNonResponseSets().FirstOrDefault(x => x.RespSetName == value);
            if (found != null)
            {
                SelectedQuestion.NRCodesS = found;
                if (!Modified.Contains(SelectedQuestion)) Modified.Add(SelectedQuestion);
        }
            else
        {

        }
                OnPropertyChanged(nameof(CurrentQuestionText));
        }

        [RelayCommand]
        private void ViewComments()
        {
            if (SelectedQuestion == null)
            {
                _dialogService.ShowError("No question selected for comments.", "Comments Error");
                return;
            }

            var comments = SelectedQuestion.Comments;
            if (comments == null || !comments.Any())
            {
                _dialogService.ShowMessage("No comments available for this question.", "Comments");
                return;
            }


        }

        [RelayCommand]
        private void ViewTranslations()
        {
            if (SelectedQuestion == null)
            {
                _dialogService.ShowError("No question selected for translation.", "Translation Error");
                return;
            }
            var translations = SelectedQuestion.Translations;
            if (translations == null || !translations.Any())
            {
                _dialogService.ShowMessage("No translations available for this question.", "Translations");
                return;
            }
            // show translation window
            TranslationVM = new TranslationViewModel(SelectedQuestion);
            _dialogService.ShowWindow(TranslationVM);
        }

        [RelayCommand]
        private void AddSurveyQuestion()
        {
            // enter VarName
            // if exists, ask user if they want to copy wordings or labels
            string newVarName = _dialogService.PromptForText("Enter VarName", "New Survey Question");

            if (string.IsNullOrWhiteSpace(newVarName))
                return;

            var existingQuestions = _surveyService.FindQuestionsByRefVarName(newVarName);
            SurveyQuestion selectedSource = null;

            if (existingQuestions.Count > 0)
            {
                selectedSource = _dialogService.PickQuestion(existingQuestions);

                SurveyQuestion newQuestion;

                if (selectedSource == null)
                {
                    newQuestion = new SurveyQuestion(newVarName);
                    newQuestion.SurveyCode = CurrentSurvey.SurveyCode;
                }
                else
                {
                    newQuestion = selectedSource;
                    newQuestion.SurveyCode = CurrentSurvey.SurveyCode;
                    newQuestion.VarName.VarName = Utilities.ChangeCC(newVarName, CurrentSurvey.CountryCode);
                    newQuestion.Qnum = "0";
                }

                CurrentSurvey.AddQuestion(newQuestion, true);
                Added.Add(newQuestion);
            }
            else
            {
                var newQuestion = new SurveyQuestion(newVarName);
                newQuestion.SurveyCode = CurrentSurvey.SurveyCode;

                CurrentSurvey.Questions.Add(newQuestion);
                Added.Add(newQuestion);
            }

            // SurveyBuilder.Refresh(); // assumes method to refresh the view
        }

        [RelayCommand]
        private void RemoveSurveyQuestion()
        {
            // ask user to document
            // ask user to save comments
            // remove from survey

            Removed.Add(SelectedQuestion);
        }

        [RelayCommand]
        private void SaveChanges()
        {
            foreach (var question in Removed)
            {
                _surveyService.RemoveQuestion(CurrentSurvey.SurveyCode, question.VarName.VarName);
                QuestionList.Remove(question);
            }
            Removed.Clear();
            foreach (var question in Added)
            {
               _surveyService.AddQuestion(question);
            }
            Added.Clear();
            foreach(var question in Modified)
            {
                _surveyService.UpdateQuestion(question);
            }
            Modified.Clear();
        }

        [RelayCommand]
        private void CopyPreviousWordings()
        {
            // get previous question in survey
            var previousQuestion = QuestionList[QuestionList.IndexOf(SelectedQuestion) - 1];

            SelectedQuestion.PrePW = new Wording(previousQuestion.PrePW.WordID, WordingType.PreP, previousQuestion.PrePW.WordingText);
            SelectedQuestion.PreIW = new Wording(previousQuestion.PreIW.WordID, WordingType.PreI, previousQuestion.PreIW.WordingText);
            SelectedQuestion.PreAW = new Wording(previousQuestion.PreAW.WordID, WordingType.PreA, previousQuestion.PreAW.WordingText);

            SelectedQuestion.PstIW = new Wording(previousQuestion.PstIW.WordID, WordingType.PstI, previousQuestion.PstIW.WordingText);
            SelectedQuestion.PstPW = new Wording(previousQuestion.PstPW.WordID, WordingType.PstP, previousQuestion.PstPW.WordingText);
            SelectedQuestion.RespOptionsS = new ResponseSet(previousQuestion.RespOptionsS.RespSetName, ResponseType.RespOptions, previousQuestion.RespOptionsS.RespList);
            SelectedQuestion.NRCodesS = new ResponseSet(previousQuestion.NRCodesS.RespSetName, ResponseType.NRCodes, previousQuestion.PrePW.WordingText);

            OnPropertyChanged(nameof(SelectedQuestion));
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        [RelayCommand]
        private void PreviousImage()
        {
            if (SelectedQuestion == null || SelectedQuestion.Images == null || !SelectedQuestion.Images.Any())
                return;
            if (CurrentImage == null)
            {
                CurrentImage = SelectedQuestion.Images.First();
                return;
            }
            int currentIndex = SelectedQuestion.Images.IndexOf(CurrentImage);
            if (currentIndex > 0)
            {
                CurrentImage = SelectedQuestion.Images[currentIndex - 1];
            }
            else
            {
                CurrentImage = SelectedQuestion.Images.Last();
            }
        }

        [RelayCommand]
        private void NextImage() { 
            if (SelectedQuestion == null || SelectedQuestion.Images == null || !SelectedQuestion.Images.Any())
                return;
            if (CurrentImage == null)
            {
                CurrentImage = SelectedQuestion.Images.First();
                return;
            }
            int currentIndex = SelectedQuestion.Images.IndexOf(CurrentImage);
            if (currentIndex < SelectedQuestion.Images.Count - 1)
            {
                CurrentImage = SelectedQuestion.Images[currentIndex + 1];
            }
            else
            {
                CurrentImage = SelectedQuestion.Images.First();
            }
        }

        [RelayCommand]
        private void OpenPreP(int wordID)
        {
            OpenWordings("PreP", wordID);
        }

        [RelayCommand]
        private void OpenPreI(int wordID)
        {
            OpenWordings("PreI", wordID);
        }

        [RelayCommand]
        private void OpenPreA(int wordID)
        {
            OpenWordings("PreA", wordID);
        }

        [RelayCommand]
        private void OpenLitQ(int wordID)
        {
            OpenWordings("LitQ", wordID);
        }

        [RelayCommand]
        private void OpenPstI(int wordID)
        {
            OpenWordings("PstI", wordID);
        }

        [RelayCommand]
        private void OpenPstP(int wordID)
        {
            OpenWordings("PstP", wordID);
        }

        [RelayCommand]
        private void OpenRespOptions(string respSetName)
        {
            OpenResponses("RespOptions", respSetName);
        }


        [RelayCommand]
        private void OpenNRCodes(string respSetName)
        {
            OpenResponses("NRCodes", respSetName);
        }

        private void OpenWordings(string type, int wordID)
        {
            WordingViewModel wordingVM = new WordingViewModel(_wordingService, _dialogService, type, wordID);
            bool? result = _dialogService.ShowDialog(wordingVM);

            if (result.Value) // if closed with OK
            {
                // apply wording to current question
                SetWording(type, wordingVM.CurrentWording);
            }
            else
            {
                UpdateWording(type);
            }
        }

        
        private void OpenResponses(string type, string setname)
        {
            ResponseSetViewModel wordingVM = new ResponseSetViewModel(_wordingService, _dialogService, type, setname);
            _dialogService.ShowDialog(wordingVM);
        }

        private void SetWording(string type, Wording? wording)
        {
            if (wording == null || SelectedQuestion == null)
                return;
            switch (type)
            {
                case "PreP":
                    SelectedQuestion.PrePW = wording;
                    PrePID = wording.WordID;
                    break;
                case "PreI":
                    SelectedQuestion.PreIW = wording;
                    PreIID = wording.WordID;
                    break;
                case "PreA":
                    SelectedQuestion.PreAW = wording;
                    PreAID = wording.WordID;
                    break;
                case "LitQ":
                    SelectedQuestion.LitQW = wording;
                    LitQID = wording.WordID;
                    break;
                case "PstI":
                    SelectedQuestion.PstIW = wording;
                    PstIID = wording.WordID;
                    break;
                case "PstP":
                    SelectedQuestion.PstPW = wording;
                    PstPID = wording.WordID;
                    break;
                default:
                    break;
            }
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        private void UpdateWording(string type)
        {
            if (SelectedQuestion == null)
                return;
            switch (type)
            {
                case "PreP":
                    SelectedQuestion.PrePW = _wordingService.GetPrePById(SelectedQuestion.PrePW.WordID);
                    break;
                case "PreI":
                    SelectedQuestion.PreIW = _wordingService.GetPreIById(SelectedQuestion.PreIW.WordID);
                    break;
                case "PreA":
                    SelectedQuestion.PreAW = _wordingService.GetPreAById(SelectedQuestion.PreAW.WordID);
                    break;
                case "LitQ":
                    SelectedQuestion.LitQW = _wordingService.GetLitQById(SelectedQuestion.LitQW.WordID);
                    break;
                case "PstI":
                    SelectedQuestion.PstIW = _wordingService.GetPstIById(SelectedQuestion.PstIW.WordID);
                    break;
                case "PstP":
                    SelectedQuestion.PstPW = _wordingService.GetPstPById(SelectedQuestion.PstPW.WordID);
                    break;
                default:
                    break;
            }
            OnPropertyChanged(nameof(CurrentQuestionText));
        }
    }
}
