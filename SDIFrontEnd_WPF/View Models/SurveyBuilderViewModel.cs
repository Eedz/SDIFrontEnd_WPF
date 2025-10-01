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
    public partial class SurveyBuilderViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ISurveyService _surveyService;
        private readonly IReferenceDataService _referenceDataService;
        private readonly IWordingService _wordingService;

        private readonly Survey CurrentSurvey;
        private readonly ObservableCollection<SurveyQuestion> _questionList;

        public List<TopicLabel> TopicLabels { get; set; }
        public List<ContentLabel> ContentLabels { get; set; }
        public List<DomainLabel> DomainLabels { get; set; }
        public List<ProductLabel> ProductLabels { get; set; }

        public ObservableCollection<SurveyQuestion> QuestionList => _questionList;

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

        }

        partial void OnPrePIDChanged(int value)
        {
            if (SelectedQuestion?.PrePW.WordID == value)
                return;

            Wording found = _wordingService.GetAllPreP().FirstOrDefault(x => x.WordID == value);
            if (found != null)
                SelectedQuestion.PrePW = found;

            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnPreIIDChanged(int value)
        {
            if (SelectedQuestion?.PreIW.WordID == value)
                return;

            Wording found = _wordingService.GetAllPreI().FirstOrDefault(x => x.WordID == value);
            if (found != null)
                SelectedQuestion.PreIW = found;

            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnPreAIDChanged(int value)
        {
            if (SelectedQuestion?.PreAW.WordID == value)
                return;

            Wording found = _wordingService.GetAllPreA().FirstOrDefault(x => x.WordID == value);
            if (found != null)
                SelectedQuestion.PreAW = found;

            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnLitQIDChanged(int value)
        {
            if (SelectedQuestion?.LitQW.WordID == value)
                return;
            Wording found = _wordingService.GetAllLitQ().FirstOrDefault(x => x.WordID == value);
            if (found != null)
                SelectedQuestion.LitQW = found;
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnPstIIDChanged(int value)
        {
            if (SelectedQuestion?.PstIW.WordID == value)
                return;
            Wording found = _wordingService.GetAllPstI().FirstOrDefault(x => x.WordID == value);
            if (found != null)
                SelectedQuestion.PstIW = found;
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnPstPIDChanged(int value)
        {
            if (SelectedQuestion?.PstPW.WordID == value)
                return;
            Wording found = _wordingService.GetAllPstP().FirstOrDefault(x => x.WordID == value);
            if (found != null)
                SelectedQuestion.PstPW = found;
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnRespNameChanged(string value)
        {
            if (SelectedQuestion?.RespOptionsS.RespSetName == value)
                return;
            ResponseSet found = _wordingService.GetAllResponseSets().FirstOrDefault(x => x.RespSetName == value);
            if (found != null)
                SelectedQuestion.RespOptionsS = found;
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnNRNameChanged(string value)
        {
            if (SelectedQuestion?.NRCodesS.RespSetName == value)
                return;
            ResponseSet found = _wordingService.GetAllNonResponseSets().FirstOrDefault(x => x.RespSetName == value);
            if (found != null)
                SelectedQuestion.NRCodesS = found;
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        [RelayCommand]
        private void EditWording(WordingType wordingType)
        {
            // open wording editor dialog

        }

        [RelayCommand]
        private void EditResponseSet(ResponseSet responseSet) 
        { 
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
            // Assuming you have a dialog to show translations
            //var translationDialog = new TranslationDialog(translations);
            //translationDialog.ShowDialog();
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


                CurrentSurvey.Questions.Add(newQuestion);
            }
            else
            {
                var newQuestion = new SurveyQuestion(newVarName);
                newQuestion.SurveyCode = CurrentSurvey.SurveyCode;

                CurrentSurvey.Questions.Add(newQuestion);
            }

            // SurveyBuilder.Refresh(); // assumes method to refresh the view
        }

        [RelayCommand]
        private void RemoveSurveyQuestion()
        {
            // ask user to document
            // ask user to save comments
            // remove from survey
            CurrentSurvey.RemoveQuestion(SelectedQuestion);

        }

        [RelayCommand]
        private void SaveChanges()
        {

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
