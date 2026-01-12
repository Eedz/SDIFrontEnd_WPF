using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using HtmlToXaml;
using ITC_Services;
using ITCLib;
using ITCReportLib;
using Microsoft.Extensions.DependencyInjection;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SDIFrontEnd_WPF.ViewModels
{
    /// <summary>
    /// View Model for main window.
    /// </summary>
    public partial class QuestionImporterViewModel : ViewModelBase
    {
        #region Fields

        private readonly IFileDialogService _fileDialogService;
        private readonly ISurveyService _surveyService;

        private readonly IPeopleService _peopleService;
        private readonly ICommentService _commentService;
        private readonly IWordingService _wordingService;
        private readonly IVarNameService _varNameService;
        private readonly QuestionImporterService _questionImporterService;

        [ObservableProperty]
        private Survey? surveyCode;

        [ObservableProperty]
        private string importStatus;

        [ObservableProperty]
        private List<Survey> surveyList;
        [ObservableProperty]
        private List<Person> peopleList;
        [ObservableProperty]
        private List<CommentType> commentTypeList;

        [ObservableProperty]
        private List<Wording> prePList;
        [ObservableProperty]
        private List<Wording> preIList;
        [ObservableProperty]
        private List<Wording> preAList;
        [ObservableProperty]
        private List<Wording> litQList;
        [ObservableProperty]
        private List<Wording> pstIList;
        [ObservableProperty]
        private List<Wording> pstPList;
        [ObservableProperty]
        private List<ResponseSet> rOList;
        [ObservableProperty]
        private List<ResponseSet> nRList;


        List<SurveyQuestion> ExistingQuestions;

        public ObservableCollection<QuestionCandidatePreview> Questions = new ObservableCollection<QuestionCandidatePreview>();
        public ObservableCollection<QuestionCandidatePreview> ApprovedQuestions { get; set; } = new ObservableCollection<QuestionCandidatePreview>();
        public QuestionCandidatePreview SelectedApprovedQuestion { get; set; }
        public List<SurveyQuestion> savedQuestions = new List<SurveyQuestion>();

        ICollectionView questionsView;
        ICollectionView approvedQuestionsView;

        private bool _isPopupOpen;
        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set
            {
                SetProperty(ref _isPopupOpen, value);
                Opacity = _isPopupOpen ? 0.7 : 1.0;
                BlurFactor = _isPopupOpen ? 10 : 0;
            }
        }

        [ObservableProperty]
        private double opacity = 1.0;

        [ObservableProperty]
        private int blurFactor = 0;

        private QuestionCandidatePreview? _currentQuestion;
        public QuestionCandidatePreview? CurrentQuestion
        {
            get => _currentQuestion;
            set
            {
                if (_currentQuestion != value)
                {
                    if (CurrentQuestion != null)
                        UnsubscribeToCollectionChanged();

                    _currentQuestion = value;

                    if (CurrentQuestion != null)
                        SubscribeToCollectionChanged();

                    OnPropertyChanged(nameof(CurrentQuestion));
                }
            }
        }

        private QuestionCandidatePreview? _currentApprovedQuestion;
        public QuestionCandidatePreview? CurrentApprovedQuestion
        {
            get => _currentApprovedQuestion;
            set
            {
                if (_currentApprovedQuestion != value)
                {
                    _currentApprovedQuestion = value;
                    OnPropertyChanged(nameof(CurrentApprovedQuestion));
                }
            }
        }

        public string QuestionPreview 
        { 
            get =>
                CurrentQuestion == null ? "No question to display" : new SurveyQuestion()
                {
                    PrePW = new Wording(-1, WordingType.PreP, CurrentQuestion.Revised.PreP.Text),
                    PreIW = new Wording(-1, WordingType.PreI, CurrentQuestion.Revised.PreI.Text),
                    PreAW = new Wording(-1, WordingType.PreA, CurrentQuestion.Revised.PreA.Text),
                    LitQW = new Wording(-1, WordingType.LitQ, CurrentQuestion.Revised.LitQ.Text),
                    PstIW = new Wording(-1, WordingType.PstI, CurrentQuestion.Revised.PstI.Text),
                    PstPW = new Wording(-1, WordingType.PstP, CurrentQuestion.Revised.PstP.Text),
                    RespOptionsS = new ResponseSet("-1", ResponseType.RespOptions, CurrentQuestion.Revised.RespOptions.Text),
                    NRCodesS = new ResponseSet("-1", ResponseType.NRCodes, CurrentQuestion.Revised.NRCodes.Text)
                }.GetQuestionTextHTML().Replace(" ", "&nbsp;");
        }

        public string SubQuestionPreview
        {
            get =>
                CurrentApprovedQuestion == null ? "No question to display" : new SurveyQuestion()
                {
                    PrePW = new Wording(-1, WordingType.PreP, CurrentApprovedQuestion.Revised.PreP.Text),
                    PreIW = new Wording(-1, WordingType.PreI, CurrentApprovedQuestion.Revised.PreI.Text),
                    PreAW = new Wording(-1, WordingType.PreA, CurrentApprovedQuestion.Revised.PreA.Text),
                    LitQW = new Wording(-1, WordingType.LitQ, CurrentApprovedQuestion.Revised.LitQ.Text),
                    PstIW = new Wording(-1, WordingType.PstI, CurrentApprovedQuestion.Revised.PstI.Text),
                    PstPW = new Wording(-1, WordingType.PstP, CurrentApprovedQuestion.Revised.PstP.Text),
                    RespOptionsS = new ResponseSet("-1", ResponseType.RespOptions, CurrentApprovedQuestion.Revised.RespOptions.Text),
                    NRCodesS = new ResponseSet("-1", ResponseType.NRCodes, CurrentApprovedQuestion.Revised.NRCodes.Text)
                }.GetQuestionTextHTML().Replace(" ", "&nbsp;");
        }

        public int TotalRecords { get => Questions.Count; }
        public int CurrentRecordNumber { get => currentIndex + 1; }

        public int TotalSubRecords { get => ApprovedQuestions.Count; }
        public int CurrentSubRecordNumber { get => currentSubIndex + 1; }

        int currentIndex;
        public int CurrentIndex
        {
            get => currentIndex;
            set
            {
                if (currentIndex != value)
                {
                    currentIndex = value;
                    if (currentIndex < 0)
                        currentIndex = 0;

                    OnPropertyChanged(nameof(CurrentRecordNumber));

                    CurrentQuestion = (QuestionCandidatePreview)questionsView.CurrentItem;
                    
                    OnPropertyChanged(nameof(QuestionPreview));
                    OnPropertyChanged(nameof(CurrentQuestion));
                    ApproveQuestionCommand?.NotifyCanExecuteChanged();
                    MoveNextCommand?.NotifyCanExecuteChanged();
                    MovePreviousCommand?.NotifyCanExecuteChanged();
                    MoveLastCommand?.NotifyCanExecuteChanged();
                    MoveFirstCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        int currentSubIndex;
        public int CurrentSubIndex
        {
            get => currentSubIndex;
            set
            {
                if (currentSubIndex != value)
                {
                    currentSubIndex = value;
                    if (currentSubIndex < 0)
                        currentSubIndex = 0;

                    OnPropertyChanged(nameof(CurrentSubRecordNumber));

                    CurrentApprovedQuestion = (QuestionCandidatePreview)approvedQuestionsView.CurrentItem;
                    OnPropertyChanged(nameof(CurrentApprovedQuestion));
                    MoveNextSubCommand?.NotifyCanExecuteChanged();
                    MovePreviousSubCommand?.NotifyCanExecuteChanged();
                    MoveLastSubCommand?.NotifyCanExecuteChanged();
                    MoveFirstSubCommand?.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(SubQuestionPreview));                  
                }
            }
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ImportDataCommand))]
        private string? sourceFilePath;

        #endregion // Fields

        #region Constructor

        public QuestionImporterViewModel(IFileDialogService dialogService, ISurveyService surveySerivce,
                                        IPeopleService peopleService, ICommentService commentService,
                                        IWordingService wordingService, IVarNameService varnameService, QuestionImporterService questionImporterService)
        {
            base.DisplayName = "Survey Importer";

            _fileDialogService = dialogService;
            _surveyService = surveySerivce;

            _peopleService = peopleService;
            _commentService = commentService;
;
            _wordingService = wordingService;
            _varNameService = varnameService;
            _questionImporterService = questionImporterService;

            LoadData();

            ExistingQuestions = new List<SurveyQuestion>();

            questionsView = CollectionViewSource.GetDefaultView(Questions);
            approvedQuestionsView = CollectionViewSource.GetDefaultView(ApprovedQuestions);
            CurrentQuestion = new QuestionCandidatePreview();
            CurrentApprovedQuestion = new QuestionCandidatePreview();

            SurveyCode = null;
            SourceFilePath = string.Empty;
            ImportStatus = "Ready.";

            currentIndex = -1;
            currentSubIndex = -1;

            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(CurrentApprovedQuestion));
        }

        private void PreP_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _questionImporterService.SetWording(CurrentQuestion.Revised.PreP);
            SetSeriesParts(CurrentQuestion);
            OnPropertyChanged(nameof(QuestionPreview));
        }

        private void PreI_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _questionImporterService.SetWording(CurrentQuestion.Revised.PreI);
            SetSeriesParts(CurrentQuestion);
            OnPropertyChanged(nameof(QuestionPreview));
        }

        private void PreA_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _questionImporterService.SetWording(CurrentQuestion.Revised.PreA);
            SetSeriesParts(CurrentQuestion);
            OnPropertyChanged(nameof(QuestionPreview));
        }

        private void LitQ_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _questionImporterService.SetWording(CurrentQuestion.Revised.LitQ);
            SetSeriesParts(CurrentQuestion);
            OnPropertyChanged(nameof(QuestionPreview));
        }

        private void PstI_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _questionImporterService.SetWording(CurrentQuestion.Revised.PstI);
            SetSeriesParts(CurrentQuestion);
            OnPropertyChanged(nameof(QuestionPreview));
        }

        private void PstP_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _questionImporterService.SetWording(CurrentQuestion.Revised.PstP);
            SetSeriesParts(CurrentQuestion);
            OnPropertyChanged(nameof(QuestionPreview));
        }

        private void RespOptions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _questionImporterService.SetResponseSet(CurrentQuestion.Revised.RespOptions);
            SetSeriesParts(CurrentQuestion);
            OnPropertyChanged(nameof(QuestionPreview));
        }

        private void NRCodes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _questionImporterService.SetResponseSet(CurrentQuestion.Revised.NRCodes);
            SetSeriesParts(CurrentQuestion);
            OnPropertyChanged(nameof(QuestionPreview));
        }

        private void UnsubscribeToCollectionChanged()
        {
            if (CurrentQuestion == null)
                return;
            CurrentQuestion.Revised.PreP.Lines.CollectionChanged -= PreP_CollectionChanged;
            CurrentQuestion.Revised.PreI.Lines.CollectionChanged -= PreI_CollectionChanged;
            CurrentQuestion.Revised.PreA.Lines.CollectionChanged -= PreA_CollectionChanged;
            CurrentQuestion.Revised.LitQ.Lines.CollectionChanged -= LitQ_CollectionChanged;
            CurrentQuestion.Revised.PstI.Lines.CollectionChanged -= PstI_CollectionChanged;
            CurrentQuestion.Revised.PstP.Lines.CollectionChanged -= PstP_CollectionChanged;
            CurrentQuestion.Revised.RespOptions.Lines.CollectionChanged -= RespOptions_CollectionChanged;
            CurrentQuestion.Revised.NRCodes.Lines.CollectionChanged -= RespOptions_CollectionChanged;
        }

        private void SubscribeToCollectionChanged()
        {
            if (CurrentQuestion == null)
                return;
            CurrentQuestion.Revised.PreP.Lines.CollectionChanged += PreP_CollectionChanged;
            CurrentQuestion.Revised.PreI.Lines.CollectionChanged += PreI_CollectionChanged;
            CurrentQuestion.Revised.PreA.Lines.CollectionChanged += PreA_CollectionChanged;
            CurrentQuestion.Revised.LitQ.Lines.CollectionChanged += LitQ_CollectionChanged;
            CurrentQuestion.Revised.PstI.Lines.CollectionChanged += PstI_CollectionChanged;
            CurrentQuestion.Revised.PstP.Lines.CollectionChanged += PstP_CollectionChanged;
            CurrentQuestion.Revised.RespOptions.Lines.CollectionChanged += RespOptions_CollectionChanged;
            CurrentQuestion.Revised.NRCodes.Lines.CollectionChanged += RespOptions_CollectionChanged;
        }

        #endregion // Constructor

        /// <summary>
        /// For debugging purposes, this method is used to run the importer with a specific survey code and file path.
        /// </summary>
        /// <param name="surveycode"></param>
        /// <param name="file"></param>
        private void RunFile(Survey surveycode, string file)
        {
            SurveyCode = surveycode;
            SourceFilePath = file;
            ImportData(SurveyCode.SurveyCode);
        }

        

        #region Commands
        [RelayCommand]
        void ChooseFile()
        {
            SourceFilePath = _fileDialogService.OpenFile("Word documents (*.docx)|*.docx");
        }

        [RelayCommand(CanExecute = nameof(CanImportData))]
        void ImportData(string surveycode)
        {
            ApprovedQuestions.Clear();

            try
            {
                var imported = _questionImporterService.ImportQuestions(surveycode, SourceFilePath);


                if (imported.Count() == 0)
                {
                    ImportStatus = "No questions found to import.";
                    return;
                }

                Questions = new ObservableCollection<QuestionCandidatePreview>(imported);
                questionsView = CollectionViewSource.GetDefaultView(Questions);

                CurrentIndex = -1;
                CurrentIndex = 0;

                ImportStatus = "Import successful.";
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(TotalRecords));

                MoveNextCommand?.NotifyCanExecuteChanged();
                MovePreviousCommand?.NotifyCanExecuteChanged();
                MoveLastCommand?.NotifyCanExecuteChanged();
                MoveFirstCommand?.NotifyCanExecuteChanged();
                ApproveQuestionCommand?.NotifyCanExecuteChanged();
            }
            catch (WrongColumnsException e)
            {
                ImportStatus = "Unable to import. Column headers are not correct.";
                return;
            }
            catch (EmptyRowsException e)
            {
                ImportStatus = "No questions found to import.";
                return;
            }
            catch (MissingTableException e)
            {
                ImportStatus = "Unable to import. No table found.";
                return;
            }
        }
        
        bool CanImportData()
        {
            return !string.IsNullOrEmpty(SourceFilePath) && SurveyCode != null && SurveyCode.SurveyCode != null;
        }

        [RelayCommand(CanExecute = nameof(CanMoveNext))]
        private void MoveNext()
        {
            if (questionsView.MoveCurrentToNext())
            {
                CurrentIndex = questionsView.CurrentPosition;
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(CurrentIndex));
            }
        }
        private bool CanMoveNext()
        {
            return CurrentIndex < Questions.Count - 1;
        } 

        [RelayCommand(CanExecute = nameof(CanMovePrevious))]
        private void MovePrevious()
        {
            if (questionsView.MoveCurrentToPrevious())
            {
                CurrentIndex = questionsView.CurrentPosition;
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(CurrentIndex));
            }
        }
        private bool CanMovePrevious()
        {
            return  CurrentIndex > 0;
        } 

        [RelayCommand(CanExecute = nameof(CanMoveFirst))]
        private void MoveFirst()
        {
            if (questionsView.MoveCurrentToFirst())
            {
                CurrentIndex = questionsView.CurrentPosition;
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(CurrentIndex));
            }
        }
        private bool CanMoveFirst()
        {
            return CurrentIndex > 0;
        } 

        [RelayCommand(CanExecute = nameof(CanMoveLast))]
        private void MoveLast()
        {
            if (questionsView.MoveCurrentToLast())
            {
                CurrentIndex = questionsView.CurrentPosition;
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(CurrentIndex));
            }
        }
        private bool CanMoveLast() {
            return CurrentIndex < Questions.Count - 1;
        }
       
        [RelayCommand]
        private void RefreshCurrent()
        {
            SuggestWordings(CurrentQuestion.Revised);
            SetSeriesParts(CurrentQuestion);
            OnPropertyChanged(nameof(QuestionPreview));
        }

        [RelayCommand(CanExecute =nameof(CanApproveQuestion))]
        void ApproveQuestion()
        {
            if (!ApprovedQuestions.Contains(CurrentQuestion))
                ApprovedQuestions.Add(CurrentQuestion);

            OnPropertyChanged(nameof(TotalSubRecords));
            OnPropertyChanged(nameof(ApprovedQuestions));
            SelectedApprovedQuestion = CurrentQuestion;
            OnPropertyChanged(nameof(SelectedApprovedQuestion));
            ShowResultsCommand.NotifyCanExecuteChanged();
            SaveQuestionsCommand.NotifyCanExecuteChanged();
        }

        private bool CanApproveQuestion()
        {
            return CurrentQuestion != null && (!CurrentQuestion.NoChanges() || CurrentQuestion.Comments.Count > 0);
        }

        [RelayCommand(CanExecute = nameof(CanShowResults))]
        private void ShowResults()
        {
            approvedQuestionsView = CollectionViewSource.GetDefaultView(ApprovedQuestions);
            approvedQuestionsView.MoveCurrentToFirst();
            CurrentApprovedQuestion = (QuestionCandidatePreview)approvedQuestionsView.CurrentItem;

            CurrentSubIndex = 0;
            MoveNextSubCommand.NotifyCanExecuteChanged();
            MovePreviousSubCommand.NotifyCanExecuteChanged();
            MoveLastSubCommand.NotifyCanExecuteChanged();
            MoveFirstSubCommand.NotifyCanExecuteChanged();


            OnPropertyChanged(nameof(CurrentApprovedQuestion));
            OnPropertyChanged(nameof(SubQuestionPreview));

            IsPopupOpen = true;

            HideResultsCommand.NotifyCanExecuteChanged();

        }

        bool CanShowResults()
        {
            return ApprovedQuestions.Count > 0 && !IsPopupOpen;
        }

        [RelayCommand(CanExecute = nameof(CanSaveQuestions))]
        /// <summary>
        /// Save each approved question candidate to the database and add it to the list of saved questions if succuessful.
        /// </summary>
        void SaveQuestions()
        {
            foreach (QuestionCandidatePreview candidate in ApprovedQuestions)
            {

                if (candidate.SaveWordings)
                    // check each field for new wordings and create them.
                    CreateNewWordings();


                // process the candidate and return the resulting SurveyQuestion object or null if the operation failed.
                SurveyQuestion? question = ProcessCandidate(candidate);

                if (question == null)
                    continue;

                Questions.Remove(candidate);
                savedQuestions.Add(question);

            }
            ApprovedQuestions.Clear();
            OnPropertyChanged(nameof(TotalSubRecords));
            OnPropertyChanged(nameof(CurrentApprovedQuestion));
            CurrentIndex = 0;
            HideResults();
        }
        private bool CanSaveQuestions()
        {
            return ApprovedQuestions.Count > 0;
        }

        [RelayCommand(CanExecute = nameof(CanHideResults))]
        /// <summary>
        /// Set IsPopupOpen to false, go back to first record.
        /// </summary>
        private void HideResults()
        {
            IsPopupOpen = false;
            CurrentIndex = -1;
            CurrentIndex = 0;
            OnPropertyChanged(nameof(QuestionPreview));
            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(TotalRecords));
            OnPropertyChanged(nameof(CurrentRecordNumber));
        }

        bool CanHideResults()
        {
            return IsPopupOpen;
        }

        [RelayCommand(CanExecute =nameof(CanMoveNextSub))]
        private void MoveNextSub()
        {
            if (approvedQuestionsView.MoveCurrentToNext())
            {
                CurrentSubIndex = approvedQuestionsView.CurrentPosition;
                OnPropertyChanged(nameof(CurrentApprovedQuestion));
                OnPropertyChanged(nameof(CurrentSubIndex));
            }
        }

        bool CanMoveNextSub()
        {
            return CurrentSubIndex < ApprovedQuestions.Count - 1;
        }

        [RelayCommand(CanExecute =nameof(CanMovePreviousSub))]
        private void MovePreviousSub()
        {
            if (approvedQuestionsView.MoveCurrentToPrevious())
            {
                CurrentSubIndex = approvedQuestionsView.CurrentPosition;
                OnPropertyChanged(nameof(CurrentApprovedQuestion));
                OnPropertyChanged(nameof(CurrentSubIndex));
            }
        }
        bool CanMovePreviousSub()
        {
            return CurrentSubIndex > 0;
        }

        [RelayCommand(CanExecute =nameof(CanMoveFirstSub))]
        private void MoveFirstSub()
        {
            if (approvedQuestionsView.MoveCurrentToFirst())
            {
                CurrentSubIndex = approvedQuestionsView.CurrentPosition;
                OnPropertyChanged(nameof(CurrentApprovedQuestion));
                OnPropertyChanged(nameof(CurrentSubIndex));
            }
        }

        bool CanMoveFirstSub()
        {
            return CurrentSubIndex > 0;
        }

        [RelayCommand(CanExecute =nameof(CanMoveLastSub))]
        private void MoveLastSub()
        {
            if (approvedQuestionsView.MoveCurrentToLast())
            {
                CurrentSubIndex = approvedQuestionsView.CurrentPosition;
                OnPropertyChanged(nameof(CurrentApprovedQuestion));
                OnPropertyChanged(nameof(CurrentSubIndex));
            }
        }
        bool CanMoveLastSub()
        {
            return CurrentSubIndex < ApprovedQuestions.Count - 1;
        }

        #endregion // Commands

        #region Private Helpers
        void LoadData()
        {
            SurveyList = _surveyService.GetAllSurveys();
            PeopleList = _peopleService.GetPeopleBasics();
            CommentTypeList = _commentService.GetAllCommentTypes();

            PrePList = _wordingService.GetAllPreP();
            PreIList = _wordingService.GetAllPreI();
            PreAList = _wordingService.GetAllPreA();
            LitQList = _wordingService.GetAllLitQ();
            PstIList = _wordingService.GetAllPstI();
            PstPList = _wordingService.GetAllPstP();
            ROList = _wordingService.GetAllResponseSets();
            NRList = _wordingService.GetAllNonResponseSets();

        }

        private void SetSeriesParts(QuestionCandidatePreview questionCandidate)
        {
            var matchingQuestion = ExistingQuestions.FirstOrDefault(x => x.VarName.VarName.Equals(questionCandidate.VarName));
            if (matchingQuestion != null && !string.IsNullOrEmpty(questionCandidate.Qnum) && char.IsLetter(questionCandidate.Qnum[questionCandidate.Qnum.Length - 1]) && questionCandidate.Qnum[questionCandidate.Qnum.Length - 1] != 'a')
            {
                if (string.IsNullOrEmpty(questionCandidate.Original.PreP.Text)) questionCandidate.Original.PreP = new WordingCandidate(WordingType.PreP, matchingQuestion.PrePW.WordID, matchingQuestion.PrePW.WordingText);
                if (string.IsNullOrEmpty(questionCandidate.Original.PreI.Text)) questionCandidate.Original.PreI = new WordingCandidate(WordingType.PreI, matchingQuestion.PreIW.WordID, matchingQuestion.PreIW.WordingText);
                if (string.IsNullOrEmpty(questionCandidate.Original.PreA.Text)) questionCandidate.Original.PreA = new WordingCandidate(WordingType.PreA, matchingQuestion.PreAW.WordID, matchingQuestion.PreAW.WordingText);
                if (string.IsNullOrEmpty(questionCandidate.Original.RespOptions.Text)) questionCandidate.Original.RespOptions = new ResponseSetCandidate(ResponseType.RespOptions, matchingQuestion.RespOptionsS.RespSetName, matchingQuestion.RespOptionsS.RespList);
                if (string.IsNullOrEmpty(questionCandidate.Original.NRCodes.Text)) questionCandidate.Original.NRCodes = new ResponseSetCandidate(ResponseType.NRCodes, matchingQuestion.NRCodesS.RespSetName, matchingQuestion.NRCodesS.RespList);
                if (string.IsNullOrEmpty(questionCandidate.Original.PstI.Text)) questionCandidate.Original.PstI = new WordingCandidate(WordingType.PstI, matchingQuestion.PstIW.WordID, matchingQuestion.PstIW.WordingText);
                if (string.IsNullOrEmpty(questionCandidate.Original.PstP.Text)) questionCandidate.Original.PstP = new WordingCandidate(WordingType.PstP, matchingQuestion.PstPW.WordID, matchingQuestion.PstPW.WordingText);

                if (!questionCandidate.IsDeletion)
                {
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PreP.Text)) questionCandidate.Revised.PreP = new WordingCandidate(WordingType.PreP, matchingQuestion.PrePW.WordID, matchingQuestion.PrePW.WordingText);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PreI.Text)) questionCandidate.Revised.PreI = new WordingCandidate(WordingType.PreI, matchingQuestion.PreIW.WordID, matchingQuestion.PreIW.WordingText);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PreA.Text)) questionCandidate.Revised.PreA = new WordingCandidate(WordingType.PreA, matchingQuestion.PreAW.WordID, matchingQuestion.PreAW.WordingText);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.RespOptions.Text)) questionCandidate.Revised.RespOptions = new ResponseSetCandidate(ResponseType.RespOptions, matchingQuestion.RespOptionsS.RespSetName, matchingQuestion.RespOptionsS.RespList);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.NRCodes.Text)) questionCandidate.Revised.NRCodes = new ResponseSetCandidate(ResponseType.NRCodes, matchingQuestion.NRCodesS.RespSetName, matchingQuestion.NRCodesS.RespList);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PstI.Text)) questionCandidate.Revised.PstI = new WordingCandidate(WordingType.PstI, matchingQuestion.PstIW.WordID, matchingQuestion.PstIW.WordingText);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PstP.Text)) questionCandidate.Revised.PstP = new WordingCandidate(WordingType.PstP, matchingQuestion.PstPW.WordID, matchingQuestion.PstPW.WordingText);
                }
            }

            if (questionCandidate.IsNewQuestion && char.IsLetter(questionCandidate.Qnum[questionCandidate.Qnum.Length - 1]))
            {
                var seriesStarter = ExistingQuestions.FirstOrDefault(x => x.Qnum.Substring(0, 3).Equals(questionCandidate.Qnum.Substring(0, 3)));
                if (seriesStarter != null)
                {
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PreP.Text)) questionCandidate.Revised.PreP = new WordingCandidate(WordingType.PreP, seriesStarter.PrePW.WordID, seriesStarter.PrePW.WordingText);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PreI.Text)) questionCandidate.Revised.PreI = new WordingCandidate(WordingType.PreI, seriesStarter.PreIW.WordID, seriesStarter.PreIW.WordingText);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PreA.Text)) questionCandidate.Revised.PreA = new WordingCandidate(WordingType.PreA, seriesStarter.PreAW.WordID, seriesStarter.PreAW.WordingText);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.RespOptions.Text)) questionCandidate.Revised.RespOptions = new ResponseSetCandidate(ResponseType.RespOptions, seriesStarter.RespOptionsS.RespSetName, seriesStarter.RespOptionsS.RespList);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.NRCodes.Text)) questionCandidate.Revised.NRCodes = new ResponseSetCandidate(ResponseType.NRCodes, seriesStarter.NRCodesS.RespSetName, seriesStarter.NRCodesS.RespList);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PstI.Text)) questionCandidate.Revised.PstI = new WordingCandidate(WordingType.PstI, seriesStarter.PstIW.WordID, seriesStarter.PstIW.WordingText);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PstP.Text)) questionCandidate.Revised.PstP = new WordingCandidate(WordingType.PstP, seriesStarter.PstPW.WordID, seriesStarter.PstPW.WordingText);
                }else
                {
                    var seriesStarter2 = Questions.FirstOrDefault(x => x.Qnum.Substring(0, 3).Equals(questionCandidate.Qnum.Substring(0, 3)));
                    if (seriesStarter2 != null)
                    {
                        if (string.IsNullOrEmpty(questionCandidate.Revised.PreP.Text)) questionCandidate.Revised.PreP = new WordingCandidate(WordingType.PreP, seriesStarter2.Revised.PreP.WordID, seriesStarter2.Revised.PreP.Text);
                        if (string.IsNullOrEmpty(questionCandidate.Revised.PreI.Text)) questionCandidate.Revised.PreI = new WordingCandidate(WordingType.PreI, seriesStarter2.Revised.PreI.WordID, seriesStarter2.Revised.PreI.Text);
                        if (string.IsNullOrEmpty(questionCandidate.Revised.PreA.Text)) questionCandidate.Revised.PreA = new WordingCandidate(WordingType.PreA, seriesStarter2.Revised.PreA.WordID, seriesStarter2.Revised.PreA.Text);
                        if (string.IsNullOrEmpty(questionCandidate.Revised.RespOptions.Text)) questionCandidate.Revised.RespOptions = new ResponseSetCandidate(ResponseType.RespOptions, seriesStarter2.Revised.RespOptions.SetName, seriesStarter2.Revised.RespOptions.Text);
                        if (string.IsNullOrEmpty(questionCandidate.Revised.NRCodes.Text)) questionCandidate.Revised.NRCodes = new ResponseSetCandidate(ResponseType.NRCodes, seriesStarter2.Revised.NRCodes.SetName, seriesStarter2.Revised.NRCodes.Text);
                        if (string.IsNullOrEmpty(questionCandidate.Revised.PstI.Text)) questionCandidate.Revised.PstI = new WordingCandidate(WordingType.PstI, seriesStarter2.Revised.PstI.WordID, seriesStarter2.Revised.PstI.Text);
                        if (string.IsNullOrEmpty(questionCandidate.Revised.PstP.Text)) questionCandidate.Revised.PstP = new WordingCandidate(WordingType.PstP, seriesStarter2.Revised.PstP.WordID, seriesStarter2.Revised.PstP.Text);
                    }
                }
            }
        }

        

        
       

     



        SurveyQuestion? ProcessCandidate(QuestionCandidatePreview candidate)
        {
            int result = 0;
            // make new SurveyQuestion object from the candidate
            SurveyQuestion question = CreateQuestion(candidate);

            if (question == null)
                return null;

            if (candidate.IsDeletion)
            {
                result = ProcessDeletion(question);
            }
            else
            {
                if (candidate.IsNewQuestion)
                {
                    result = ProcessNewQuestion(question);
                }
                else
                {
                    result = ProcessExistingQuestion(question, !candidate.NoChanges());
                }

                if (candidate.SaveComments)
                    ProcessQuestionComments(question);
            }
            if (result == 1)
                return null;

            return question;
        }

        /// <summary>
        /// Creates or Updates wordings in the database for each field that has a new wording. The candidate's wording ID is updated with the new or existing wording ID.
        /// </summary>
        void CreateNewWordings()
        {
            foreach (QuestionCandidatePreview candidate in ApprovedQuestions)
            { 
                if (candidate.Revised.PreP.NewWording)
                {
                    var match = PrePList.FirstOrDefault(x => x.WordingText.Equals(candidate.Revised.PreP.Text));
                    if (match == null)
                    {
                        var uses = _wordingService.GetWordingUsages(new Wording(candidate.Revised.PreP.WordID, WordingType.PreP, candidate.Revised.PreP.Text));
                        if (uses.Count == 1 && uses[0].VarName == candidate.VarName && uses[0].SurveyCode == candidate.Survey)
                        {
                            candidate.Revised.PreP.WordID = uses[0].WordID;
                            UpdateWording(candidate.Revised.PreP);
                        }
                        else 
                            CreateWording(candidate.Revised.PreP);
                    }
                    else
                    {
                        candidate.Revised.PreP.WordID = match.WordID;
                        candidate.Revised.PreP.NewWording = false;
                    }
                }

                if (candidate.Revised.PreI.NewWording)
                {
                    var match = PreIList.FirstOrDefault(x => x.WordingText.Equals(candidate.Revised.PreI.Text));
                    if (match == null)
                    {
                        var uses = _wordingService.GetWordingUsages(new Wording(candidate.Revised.PreI.WordID, WordingType.PreI, candidate.Revised.PreI.Text));
                        if (uses.Count == 1 && uses[0].VarName == candidate.VarName && uses[0].SurveyCode == candidate.Survey)
                        {
                            candidate.Revised.PreI.WordID = uses[0].WordID;
                            UpdateWording(candidate.Revised.PreI);
                        }
                        else
                            CreateWording(candidate.Revised.PreI);
                    }
                    else
                    {
                        candidate.Revised.PreI.WordID = match.WordID;
                        candidate.Revised.PreI.NewWording = false;
                    }
                }

                if (candidate.Revised.PreA.NewWording)
                {
                    var match = PreAList.FirstOrDefault(x => x.WordingText.Equals(candidate.Revised.PreA.Text));
                    if (match == null)
                    {
                        var uses = _wordingService.GetWordingUsages(new Wording(candidate.Revised.PreA.WordID, WordingType.PreA, candidate.Revised.PreA.Text));
                        if (uses.Count == 1 && uses[0].VarName == candidate.VarName && uses[0].SurveyCode == candidate.Survey)
                        {
                            candidate.Revised.PreA.WordID = uses[0].WordID;
                            UpdateWording(candidate.Revised.PreA);
                        }
                        else
                            CreateWording(candidate.Revised.PreA);
                    }
                    else
                    {
                        candidate.Revised.PreA.WordID = match.WordID;
                        candidate.Revised.PreA.NewWording = false;
                    }
                }

                if (candidate.Revised.LitQ.NewWording)
                {
                    var match = LitQList.FirstOrDefault(x => x.WordingText.Equals(candidate.Revised.LitQ.Text));
                    if (match == null)
                    {
                        var uses = _wordingService.GetWordingUsages(new Wording(candidate.Revised.LitQ.WordID, WordingType.LitQ, candidate.Revised.LitQ.Text));
                        if (uses.Count == 1 && uses[0].VarName == candidate.VarName && uses[0].SurveyCode == candidate.Survey)
                        {
                            candidate.Revised.LitQ.WordID = uses[0].WordID;
                            UpdateWording(candidate.Revised.LitQ);
                        }
                        else
                            CreateWording(candidate.Revised.LitQ);
                    }
                    else
                    {
                        candidate.Revised.LitQ.WordID = match.WordID;
                        candidate.Revised.LitQ.NewWording = false;
                    }
                }

                if (candidate.Revised.PstI.NewWording)
                {
                    var match = PstIList.FirstOrDefault(x => x.WordingText.Equals(candidate.Revised.PstI.Text));
                    if (match == null)
                    {
                        var uses = _wordingService.GetWordingUsages(new Wording(candidate.Original.PstI.WordID, WordingType.PstI, candidate.Original.PstI.Text));
                        if (uses.Count == 1 && uses[0].VarName == candidate.VarName && uses[0].SurveyCode == candidate.Survey)
                        {
                            candidate.Revised.PstI.WordID = uses[0].WordID;
                            UpdateWording(candidate.Revised.PstI);
                        }
                        else
                            CreateWording(candidate.Revised.PstI);
                    }
                    else
                    {
                        candidate.Revised.PstI.WordID = match.WordID;
                        candidate.Revised.PstI.NewWording = false;
                    }
                }

                if (candidate.Revised.PstP.NewWording)
                {
                    var match = PstPList.FirstOrDefault(x => x.WordingText.Equals(candidate.Revised.PstP.Text));
                    if (match == null)
                    {
                        var uses = _wordingService.GetWordingUsages(new Wording(candidate.Original.PstP.WordID, WordingType.PstP, candidate.Original.PstP.Text));
                        if (uses.Count == 1 && uses[0].VarName == candidate.VarName && uses[0].SurveyCode == candidate.Survey)
                        {
                            candidate.Revised.PstP.WordID = uses[0].WordID;
                            UpdateWording(candidate.Revised.PstP);
                        }
                        else
                            CreateWording(candidate.Revised.PstP);
                    }
                    else
                    {
                        candidate.Revised.PstP.WordID = match.WordID;
                        candidate.Revised.PstP.NewWording = false;
                    }
                }

                if (candidate.Revised.RespOptions.NewWording)
                {
                    var match = ROList.FirstOrDefault(x => x.RespList.Equals(candidate.Revised.RespOptions.Text));
                    if (match == null)
                    {
                        var uses = _wordingService.GetResponseUsages("RespOptions", candidate.Revised.RespOptions.SetName);
                        if (uses.Count == 1 && uses[0].VarName == candidate.VarName && uses[0].SurveyCode == candidate.Survey)
                        {
                            candidate.Revised.RespOptions.SetName = uses[0].RespName;
                            UpdateResponseSet(candidate.Revised.RespOptions);
                        }
                        else
                            CreateResponseSet(candidate.Revised.RespOptions);
                    }
                    else
                    {
                        candidate.Revised.RespOptions.SetName = match.RespSetName;
                        candidate.Revised.RespOptions.NewWording = false;
                    }
                }

                if (candidate.Revised.NRCodes.NewWording)
                {
                    var match = NRList.FirstOrDefault(x => x.RespList.Equals(candidate.Revised.NRCodes.Text));
                    if (match == null)
                    {
                        var uses = _wordingService.GetResponseUsages("NRCodes", candidate.Revised.NRCodes.SetName);
                        if (uses.Count == 1 && uses[0].VarName == candidate.VarName && uses[0].SurveyCode == candidate.Survey)
                        {
                            candidate.Revised.NRCodes.SetName = uses[0].RespName;
                            UpdateResponseSet(candidate.Revised.NRCodes);
                        }
                        else
                            CreateResponseSet(candidate.Revised.NRCodes);
                    }
                    else
                    {
                        candidate.Revised.NRCodes.SetName = match.RespSetName;
                        candidate.Revised.NRCodes.NewWording = false;
                    }
                }
            }
        }

        void CreateWording(WordingCandidate wordingCandidate)
        {
            Wording newWording = new Wording(-1, wordingCandidate.FieldName, wordingCandidate.Text);
            _wordingService.InsertWording(newWording);

            switch (wordingCandidate.FieldName)
            {
                case WordingType.PreP:
                    PrePList.Add(newWording);
                    break;
                case WordingType.PreI:
                    PreIList.Add(newWording);
                    break;
                case WordingType.PreA:
                    PreAList.Add(newWording);
                    break;
                case WordingType.LitQ:
                    LitQList.Add(newWording);
                    break;
                case WordingType.PstI:
                    PstIList.Add(newWording);
                    break;
                case WordingType.PstP:
                    PstPList.Add(newWording);
                    break;
            }
        }

        void UpdateWording(WordingCandidate wordingCandidate)
        {
            Wording newWording = new Wording(wordingCandidate.WordID, wordingCandidate.FieldName, wordingCandidate.Text);
            _wordingService.UpdateWording(newWording);

            try
            {
                switch (wordingCandidate.FieldName)
                {
                    case WordingType.PreP:
                        PrePList.FirstOrDefault(x => x.WordID == wordingCandidate.WordID).WordingText = wordingCandidate.Text;
                        break;
                    case WordingType.PreI:
                        PreIList.FirstOrDefault(x => x.WordID == wordingCandidate.WordID).WordingText = wordingCandidate.Text;
                        break;
                    case WordingType.PreA:
                        PreAList.FirstOrDefault(x => x.WordID == wordingCandidate.WordID).WordingText = wordingCandidate.Text;
                        break;
                    case WordingType.LitQ:
                        LitQList.FirstOrDefault(x => x.WordID == wordingCandidate.WordID).WordingText = wordingCandidate.Text;
                        break;
                    case WordingType.PstI:
                        PstIList.FirstOrDefault(x => x.WordID == wordingCandidate.WordID).WordingText = wordingCandidate.Text;
                        break;
                    case WordingType.PstP:
                        PstPList.FirstOrDefault(x => x.WordID == wordingCandidate.WordID).WordingText = wordingCandidate.Text;
                        break;
                }
            }
            catch
            {
                // wording not found in list
            }
        }

        void UpdateResponseSet(ResponseSetCandidate candidate)
        {
            ResponseSet newWording = new ResponseSet(candidate.SetName, candidate.FieldName, candidate.Text);
            _wordingService.UpdateResponseSet(newWording);

            switch (candidate.FieldName)
            {
                case ResponseType.RespOptions:
                    ROList.FirstOrDefault(x => x.RespSetName == candidate.SetName).RespList = candidate.Text;
                    break;
                case ResponseType.NRCodes:
                    NRList.Add(newWording);
                    break;
            }
        }

        void CreateResponseSet(ResponseSetCandidate candidate)
        {
            ResponseSet newWording = new ResponseSet(candidate.SetName, candidate.FieldName, candidate.Text);
            _wordingService.InsertResponseSet(newWording);

            switch (candidate.FieldName)
            {
                case ResponseType.RespOptions:
                    ROList.Add(newWording);
                    break;
                case ResponseType.NRCodes:
                    NRList.Add(newWording);
                    break;
            }
        }

        int ProcessNewQuestion(SurveyQuestion question)
        {
            if (_varNameService.InsertVariable(new VariableName(question.VarName)) == 1)
                return 1;
            

            if (_surveyService.AddQuestion(question) !=1 && _surveyService.UpdateQuestion(question)!=1)
            {

                return 0;
            }
            else
            {
                // error inserting/updating question
                return 1;
            }

            
        }

        int ProcessNewQuestionAsync(SurveyQuestion question)
        {
            int result = _varNameService.InsertVariable(new VariableName(question.VarName));
            if (result == 1)
                return 1;

            bool addSuccess = _surveyService.AddQuestion(question) !=1;
            bool updateSuccess = _surveyService.UpdateQuestion(question) !=1;
            
            if (addSuccess && updateSuccess)
            {
                return 0;
            }
            else
            {
                // error inserting/updating question
                return 1;
            }
        }

        int ProcessQuestionComments(SurveyQuestion question)
        {
            foreach (QuestionComment comment in question.Comments)
            {
                _commentService.InsertQuestionComment(comment);
            }
            return 0;
        }

        int ProcessExistingQuestion (SurveyQuestion question, bool updateWordings)
        {
            if (updateWordings)

            {
                var result = _surveyService.UpdateQuestion(question);
                return result == 0 ? 0 : 1;
            }                

            else
                return 0;
        }

       

        int ProcessDeletion(SurveyQuestion question)
        {
            // create deleted comments first
            _commentService.BackupComments(question.ID);

            foreach (QuestionComment comment in question.Comments)
            {
                DeletedComment deleteComment = new DeletedComment()
                {
                    Survey = question.SurveyCode,
                    VarName = question.VarName.VarName,
                    Notes = comment.Notes,
                    NoteDate = comment.NoteDate,
                    Author = comment.Author,
                    NoteType = comment.NoteType,
                    Source = comment.Source,
                    Authority = comment.Authority
                };

                _commentService.InsertDeletedComment(deleteComment);
            }


            bool result = _surveyService.RemoveQuestion(question.SurveyCode, question.VarName.VarName) !=1;


            if (result)
                return 0;
            else
                return 1;
        }


        

        /// <summary>
        /// Create a new SurveyQuestion object from a QuestionCandidatePreview object.
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        SurveyQuestion CreateQuestion(QuestionCandidatePreview question)
        {
            SurveyQuestion newQuestion = new SurveyQuestion();

            newQuestion.ID = question.QID;
            newQuestion.Qnum = question.Qnum;
            newQuestion.SurveyCode = question.Survey;
            newQuestion.VarName.VarName = Utilities.ChangeCC(question.VarName, SurveyCode.CountryCode);
            newQuestion.Comments = question.Comments.ToList();

            // for each new wording, check for it in the master list
            if (question.SaveWordings)
            {
                if (question.Revised.PreP.NewWording)
                {
                    var match = PrePList.FirstOrDefault(x => x.WordingText.Equals(question.Revised.PreP.Text));
                    if (match != null)
                    {
                        newQuestion.PrePW = match;
                    }
                }
                else
                {
                    newQuestion.PrePW = new Wording(question.Revised.PreP.WordID, WordingType.PreP, question.Revised.PreP.Text);
                }

                if (question.Revised.PreI.NewWording)
                {
                    var match = PreIList.FirstOrDefault(x => x.WordingText.Equals(question.Revised.PreI.Text));
                    if (match != null)
                    {
                        newQuestion.PreIW = match;
                    }
                }
                else
                {
                    newQuestion.PreIW = new Wording(question.Revised.PreI.WordID, WordingType.PreI, question.Revised.PreI.Text);
                }

                if (question.Revised.PreA.NewWording)
                {
                    var match = PreAList.FirstOrDefault(x => x.WordingText.Equals(question.Revised.PreA.Text));
                    if (match != null)
                    {
                        newQuestion.PreAW = match;
                    }
                }
                else
                {
                    newQuestion.PreAW = new Wording(question.Revised.PreA.WordID, WordingType.PreA, question.Revised.PreA.Text);
                }

                if (question.Revised.LitQ.NewWording)
                {
                    var match = LitQList.FirstOrDefault(x => x.WordingText.Equals(question.Revised.LitQ.Text));
                    if (match != null)
                    {
                        newQuestion.LitQW = match;
                    }
                }
                else
                {
                    newQuestion.LitQW = new Wording(question.Revised.LitQ.WordID, WordingType.LitQ, question.Revised.LitQ.Text);
                }

                if (question.Revised.PstI.NewWording)
                {
                    var match = PstIList.FirstOrDefault(x => x.WordingText.Equals(question.Revised.PstI.Text));
                    if (match != null)
                    {
                        newQuestion.PstIW = match;
                    }
                }
                else
                {
                    newQuestion.PstIW = new Wording(question.Revised.PstI.WordID, WordingType.PstI, question.Revised.PstI.Text);
                }

                if (question.Revised.PstP.NewWording)
                {
                    var match = PstPList.FirstOrDefault(x => x.WordingText.Equals(question.Revised.PstP.Text));
                    if (match != null)
                    {
                        newQuestion.PstPW = match;
                    }
                }
                else
                {
                    newQuestion.PstPW = new Wording(question.Revised.PstP.WordID, WordingType.PstP, question.Revised.PstP.Text);
                }

                if (question.Revised.RespOptions.NewWording)
                {
                    var match = ROList.FirstOrDefault(x => x.RespList.Equals(question.Revised.RespOptions.Text));
                    if (match != null)
                    {
                        newQuestion.RespOptionsS = match;
                    }
                }
                else
                {
                    newQuestion.RespOptionsS = new ResponseSet(question.Revised.RespOptions.SetName, ResponseType.RespOptions, question.Revised.RespOptions.Text);
                }

                if (question.Revised.NRCodes.NewWording)
                {
                    var match = NRList.FirstOrDefault(x => x.RespList.Equals(question.Revised.NRCodes.Text));
                    if (match != null)
                    {
                        newQuestion.NRCodesS = match;
                    }
                }
                else
                {
                    newQuestion.NRCodesS = new ResponseSet(question.Revised.NRCodes.SetName, ResponseType.NRCodes, question.Revised.NRCodes.Text);
                }
            }

            return newQuestion;
        }

        /// <summary>
        /// For each wording in the candidate, set the ID/Name of the wording, or set the NewWording flag.
        /// </summary>
        /// <param name="question"></param>
        void SuggestWordings(QuestionCandidate question)
        {
            if (question == null) return;
            _questionImporterService.SetWording(question.PreP);
            _questionImporterService.SetWording(question.PreI);
            _questionImporterService.SetWording(question.PreA);
            _questionImporterService.SetWording(question.LitQ);
            _questionImporterService.SetWording(question.PstI);
            _questionImporterService.SetWording(question.PstP);
            _questionImporterService.SetResponseSet(question.RespOptions);
            _questionImporterService.SetResponseSet(question.NRCodes);
            OnPropertyChanged(nameof(CurrentQuestion));
        }


        #endregion // Private Helpers
    }
}
