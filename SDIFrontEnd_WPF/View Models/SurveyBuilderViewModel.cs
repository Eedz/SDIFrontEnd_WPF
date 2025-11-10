using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITC_Services;
using ITCLib;
using MvvmLib.ViewModels;
using SDIFrontEnd_WPF.ViewModels;
using System.Collections.ObjectModel;
namespace SDIFrontEnd_WPF
{
    public partial class SurveyBuilderViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ISurveyService _surveyService;
        private readonly IReferenceDataService _referenceDataService;
        private readonly IWordingService _wordingService;
        private readonly ICommentService _commentService;
        private readonly IPeopleService _peopleService;

        private readonly Survey CurrentSurvey;
        private readonly ObservableCollection<SurveyQuestionRecord> _recordList;
        public ObservableCollection<SurveyQuestionRecord> RecordList => _recordList;

        public ObservableCollection<SurveyQuestion> QuestionList => new ObservableCollection<SurveyQuestion>(RecordList.Select(r=>r.Item));

        public List<TopicLabel> TopicLabels { get; set; }
        public List<ContentLabel> ContentLabels { get; set; }
        public List<DomainLabel> DomainLabels { get; set; }
        public List<ProductLabel> ProductLabels { get; set; }

        public ObservableCollection<SurveyQuestion> Added { get; } = new ObservableCollection<SurveyQuestion>();
        public ObservableCollection<SurveyQuestion> Removed { get; } = new ObservableCollection<SurveyQuestion>();
        public ObservableCollection<SurveyQuestion> Modified { get; } = new ObservableCollection<SurveyQuestion>();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentQuestionText))]
        [NotifyPropertyChangedFor(nameof(SectionCount))]
        [NotifyPropertyChangedFor(nameof(SelectedQuestion))]
        [NotifyPropertyChangedFor(nameof(ItemPosition))]
        private SurveyQuestionRecord? selectedQuestionRecord;

        public ObservableCollection<SurveyQuestionRecord> SelectedQuestionRecords { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Comments))]
        private SurveyQuestion? selectedQuestion;

        [ObservableProperty]
        private ObservableCollection<SurveyQuestion>? selectedQuestions;

        public ObservableCollection<QuestionComment> Comments => new ObservableCollection<QuestionComment>(SelectedQuestion?.Comments ?? new List<QuestionComment>());

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ImageIndex))]
        private SurveyImage? currentImage;
      
        public string? ImageIndex => $"{(SelectedQuestion?.Images.IndexOf(CurrentImage) + 1)} of {SelectedQuestion?.Images.Count}";
        public string? ItemPosition => $"{(QuestionList.IndexOf(SelectedQuestion) + 1)} of {QuestionList?.Count}";



        public string CurrentQuestionText => SelectedQuestion?.GetQuestionTextHTML() ?? string.Empty;

        // other windows
        public TranslationViewModel? TranslationVM;
        public RelatedQuestionsViewModel? RelatedQsVM;

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
        private string? respName;
        [ObservableProperty]
        private string? nRName;

        public SurveyBuilderViewModel(IDialogService dialogService, ISurveyService surveyService, IReferenceDataService referenceData, IWordingService wordingService, 
            IPeopleService peopleService, ICommentService commentService, Survey survey)
        {
            _dialogService = dialogService;
            _surveyService = surveyService ?? throw new ArgumentNullException(nameof(surveyService), "Survey service cannot be null.");
            _referenceDataService = referenceData ?? throw new ArgumentNullException(nameof(referenceData), "Reference data service cannot be null.");
            _wordingService = wordingService ?? throw new ArgumentNullException(nameof(wordingService), "Wording service cannot be null.");
            _peopleService = peopleService ?? throw new ArgumentNullException(nameof(peopleService), "People service cannot be null.");
            _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService), "Comment service cannot be null.");
            if (survey == null) throw new ArgumentNullException(nameof(survey), "Questions cannot be null");

            CurrentSurvey = survey;
            _recordList = new ObservableCollection<SurveyQuestionRecord>(survey.Questions.Select(x=> new SurveyQuestionRecord(x)));

            TopicLabels= _referenceDataService.GetTopicLabels() ?? new List<TopicLabel>();
            ContentLabels= _referenceDataService.GetContentLabels() ?? new List<ContentLabel>();
            DomainLabels= _referenceDataService.GetDomainLabels() ?? new List<DomainLabel>();
            ProductLabels= _referenceDataService.GetProductLabels() ?? new List<ProductLabel>();

            SelectedQuestionRecord = _recordList.FirstOrDefault() ?? new SurveyQuestionRecord(new SurveyQuestion("Default", "0000") );
            SelectedQuestionRecords = new ObservableCollection<SurveyQuestionRecord>();
            SelectedQuestions = new ObservableCollection<SurveyQuestion>();
        }

        /// <summary>
        /// Update the form when the selected question record changes.
        /// </summary>
        /// <param name="value"></param>
        partial void OnSelectedQuestionRecordChanged(SurveyQuestionRecord? value)
        {
            if (value == null)
                return;

            SurveyQuestion question = value.Item;
            SelectedQuestion = question;
            CurrentImage = value?.Item.Images.FirstOrDefault();

            // update wordings and response sets
            PrePID = question.PrePW.WordID;
            PreIID = question.PreIW.WordID;
            PreAID = question.PreAW.WordID;
            LitQID = question.LitQW.WordID;
            PstIID = question.PstIW.WordID;
            PstPID = question.PstPW.WordID;
            RespName = question.RespOptionsS.RespSetName;
            NRName = question.NRCodesS.RespSetName;

            // update translation window
            if (TranslationVM != null)
            {

                TranslationVM.UpdateTranslations(SelectedQuestion);
                OnPropertyChanged(nameof(TranslationVM));
            }
            // update related questions window
            if (RelatedQsVM != null)
            {
                var relatedQuestions = _surveyService.FindQuestionsByRefVarName(SelectedQuestion.VarName.RefVarName);
                relatedQuestions.RemoveAll(q => q.ID == SelectedQuestion.ID); // remove current question from list
                
                RelatedQsVM.UpdateQuestions(relatedQuestions, CurrentSurvey.SurveyCodePrefix);
                OnPropertyChanged(nameof(RelatedQsVM));
            }
            
        }

        partial void OnSelectedQuestionChanged(SurveyQuestion? value)
        {
            if (value == null)
                return;
            var record = _recordList.FirstOrDefault(r => r.Item == value);
            if (record != null && record.Item.ID != SelectedQuestionRecord.Item.ID)
                SelectedQuestionRecord = record;
        }

        void ModifyPreP(Wording prep, SurveyQuestion question)
        {
            question.PrePW = prep;
            SurveyQuestionRecord record = new SurveyQuestionRecord(question);
            if (!Modified.Contains(question)) Modified.Add(question);
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnPrePIDChanged(int oldValue, int newValue)
        {
            if (SelectedQuestion?.PrePW.WordID == newValue)
                return;

            Wording? found = _wordingService.GetAllPreP().FirstOrDefault(x => x.WordID == newValue);
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

            Wording? found = _wordingService.GetAllPreI().FirstOrDefault(x => x.WordID == newValue);
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

            Wording? found = _wordingService.GetAllPreA().FirstOrDefault(x => x.WordID == newValue);
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
            Wording? found = _wordingService.GetAllLitQ().FirstOrDefault(x => x.WordID == newValue);
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
            Wording? found = _wordingService.GetAllPstI().FirstOrDefault(x => x.WordID == newValue);
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

        partial void OnRespNameChanged(string? oldValue, string? newValue)
        {
            if (SelectedQuestion == null)
                return;
            if (SelectedQuestion?.RespOptionsS.RespSetName == newValue)
                return;

            ResponseSet? found = _wordingService.GetAllResponseSets().FirstOrDefault(x => x.RespSetName == newValue);
            if (found != null)
            {
                SelectedQuestion.RespOptionsS = found;
                if (!Modified.Contains(SelectedQuestion)) Modified.Add(SelectedQuestion);
            }
            else
                RespName = oldValue;

                OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnNRNameChanged(string? oldValue, string? newValue)
        {
            if (SelectedQuestion == null) 
                return;
            if (SelectedQuestion?.NRCodesS.RespSetName == newValue)
                return;

            ResponseSet? found = _wordingService.GetAllNonResponseSets().FirstOrDefault(x => x.RespSetName == newValue);
            if (found != null)
            {
                SelectedQuestion.NRCodesS = found;
                if (!Modified.Contains(SelectedQuestion)) Modified.Add(SelectedQuestion);
            }
            else
                NRName = oldValue;

                OnPropertyChanged(nameof(CurrentQuestionText));
        }

        [RelayCommand]
        private void AddComment()
        {
            if (SelectedQuestion == null)
            {
                _dialogService.ShowError("No question selected for comments.", "Comments Error");
                return;
            }

            QuickCommentEntryViewModel vm = new QuickCommentEntryViewModel(_peopleService, _commentService, SelectedQuestion);

            bool? result = _dialogService.ShowDialog(vm);
            if (result.Value)
            {
                SelectedQuestion.Comments.Add(vm.NewComment);
                OnPropertyChanged(nameof(SelectedQuestion));
                
        }
        }

        [RelayCommand]
        private void ViewTranslations()
        {
            if (SelectedQuestion == null)
            {
                _dialogService.ShowError("No question selected.", "Translation Error");
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
        private void AddTranslation()
        {
            if (SelectedQuestion == null)
            {
                _dialogService.ShowError("No question selected.", "Translation Error");
                return;
            }
            // show translation window
            TranslationVM = new TranslationViewModel(SelectedQuestion);
            bool? result = _dialogService.ShowDialog(TranslationVM);
            if (result.Value) // if closed with OK
            {
                // save translation
                foreach (var translation in SelectedQuestion.Translations)
                {
                   // _wordingService.AddTranslation(translation);
                }
            }
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
            string newQnum = SelectedQuestion.Qnum;
            int position = QuestionList.IndexOf(SelectedQuestion);
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
                    newQuestion = new SurveyQuestion(selectedSource.VarName.VarName);
                    newQuestion.SurveyCode = CurrentSurvey.SurveyCode;
                    newQuestion.VarName.VarName = Utilities.ChangeCC(newVarName, CurrentSurvey.CountryCode);
                }
                newQuestion.Qnum = newQnum;
                CurrentSurvey.AddQuestion(newQuestion, position, true);
                Added.Add(newQuestion);
                RecordList.Insert(position, new SurveyQuestionRecord(newQuestion));
            }
            else
            {
                SurveyQuestion newQuestion = new SurveyQuestion(newVarName);
                newQuestion.VarName.VarName = Utilities.ChangeCC(newVarName, CurrentSurvey.CountryCode);
                newQuestion.SurveyCode = CurrentSurvey.SurveyCode;
                newQuestion.Qnum = newQnum;
                CurrentSurvey.AddQuestion(newQuestion, position, true);
                Added.Add(newQuestion);
                RecordList.Insert(position, new SurveyQuestionRecord(newQuestion));
            }
            OnPropertyChanged(nameof(RecordList));
        }

        [RelayCommand]
        private void AddSeries()
        {
            if (SelectedQuestion.QnumSuffix != string.Empty)
            {
                _dialogService.ShowError("Series can only be added after standalone questions.", "Add Series Error");
                return;
            }

            // open SeriesBuilder viewmodel in new window
            // get list of questions to add 
            SeriesBuilderViewModel vm = new SeriesBuilderViewModel(_referenceDataService, _wordingService);
            bool? result = _dialogService.ShowDialog(vm);

            if (result.Value)
            {
                string newQnum = SelectedQuestion.Qnum;
                int position = QuestionList.IndexOf(SelectedQuestion);
                foreach (SurveyQuestion q in vm.NewQuestions)
                {
                    q.SurveyCode = CurrentSurvey.SurveyCode;
                    q.Qnum = $"{newQnum}{q.QnumSuffix}";

                    CurrentSurvey.AddQuestion(q, position, true);
                    Added.Add(q);
                    RecordList.Insert(position, new SurveyQuestionRecord(q));
                    position++;
        }

                
                OnPropertyChanged(nameof(RecordList));
            }
        }

        [RelayCommand]
        private void RemoveSurveyQuestion()
        {
            if (SelectedQuestionRecords == null) return;
            foreach (SurveyQuestionRecord q in SelectedQuestionRecords)
            {
                Removed.Add(q.Item);                
                q.Deleted = true;
        }
        }

        [RelayCommand]
        private void SaveChanges()
        {

            if (this.Removed.Count>0)
            if (_dialogService.PromptForText("One or more questions are being deleted. Type 'DELETE' to confirm.", "Confirm Deletes") != "DELETE")
            {
                _dialogService.ShowMessage("Failed to confirm deletes. Changes have not been saved.");
                return; 
            }
            else
            {
                ProcessDeletes();
            }

            foreach (var r in RecordList.Where(x => x.ShouldSave || x.Deleted || x.NewRecord))
            {
                _surveyService.SaveQuestion(r);
            }

            OnPropertyChanged(nameof(RecordList));
        }

        [RelayCommand]
        private void ViewRelatedQuestions()
        {
            if (SelectedQuestion == null)
            {
                _dialogService.ShowError("No question selected.", "Related Questions Error");
                return;
            }
            var relatedQuestions = _surveyService.FindQuestionsByRefVarName(SelectedQuestion.VarName.RefVarName);
            relatedQuestions.RemoveAll(q => q.ID == SelectedQuestion.ID); // remove current question from list
            if (relatedQuestions == null || !relatedQuestions.Any())
            {
                _dialogService.ShowMessage("No related questions found.", "Related Questions");
                return;
            }
            // show related questions window
            RelatedQsVM = new RelatedQuestionsViewModel(relatedQuestions, CurrentSurvey.SurveyCodePrefix);
            _dialogService.ShowWindow(RelatedQsVM);
        }

        [RelayCommand]
        private void ViewDeletedVars()
        {
            var deletes = _surveyService.GetDeletedQuestions(CurrentSurvey.SurveyCode);
            // show related questions window
            DeletedQuestionsViewModel deletedVM = new DeletedQuestionsViewModel(deletes);
            _dialogService.ShowWindow(deletedVM);
        }

            

        [RelayCommand]
        private void CopyPreviousWordings()
        {
            if (SelectedQuestion == null)                 return;

            // get previous question in survey
            var previousQuestion = QuestionList.Where(x=>x.GetQnumValue() < SelectedQuestion.GetQnumValue()).LastOrDefault();

            if (previousQuestion == null)
            {
                _dialogService.ShowError("No previous question found in the survey.", "Copy Wordings Error");
                return;
            }

            SelectedQuestion.PrePW = new Wording(previousQuestion.PrePW.WordID, WordingType.PreP, previousQuestion.PrePW.WordingText);
            SelectedQuestion.PreIW = new Wording(previousQuestion.PreIW.WordID, WordingType.PreI, previousQuestion.PreIW.WordingText);
            SelectedQuestion.PreAW = new Wording(previousQuestion.PreAW.WordID, WordingType.PreA, previousQuestion.PreAW.WordingText);
            // do not copy LitQ as it is unique to the question
            SelectedQuestion.PstIW = new Wording(previousQuestion.PstIW.WordID, WordingType.PstI, previousQuestion.PstIW.WordingText);
            SelectedQuestion.PstPW = new Wording(previousQuestion.PstPW.WordID, WordingType.PstP, previousQuestion.PstPW.WordingText);
            SelectedQuestion.RespOptionsS = new ResponseSet(previousQuestion.RespOptionsS.RespSetName, ResponseType.RespOptions, previousQuestion.RespOptionsS.RespList);
            SelectedQuestion.NRCodesS = new ResponseSet(previousQuestion.NRCodesS.RespSetName, ResponseType.NRCodes, previousQuestion.PrePW.WordingText);

            OnPropertyChanged(nameof(SelectedQuestion));
            OnPropertyChanged(nameof(CurrentQuestionText));
        }


        #region Navigation Commands
        [RelayCommand]
        private void FirstImage()
        {
            if (SelectedQuestion == null || SelectedQuestion.Images == null || !SelectedQuestion.Images.Any())
                return;
            CurrentImage = SelectedQuestion.Images.First();
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
        private void LastImage()
        {
            if (SelectedQuestion == null || SelectedQuestion.Images == null || !SelectedQuestion.Images.Any())
                return;
            CurrentImage = SelectedQuestion.Images.Last();
        }

        [RelayCommand]
        private void PreviousItem()
        {
            if (QuestionList == null)
                return;
            if (SelectedQuestion == null)
            {
                SelectedQuestion = QuestionList.FirstOrDefault();
                return;
            }
            int currentIndex = QuestionList.IndexOf(SelectedQuestion);
            if (currentIndex > 0)
            {
                SelectedQuestion = QuestionList[currentIndex - 1];
            }
            else
            {
                SelectedQuestion = QuestionList.Last();
            }
        }

        [RelayCommand]
        private void NextItem()
        {
            if (QuestionList == null)
                return;
            if (SelectedQuestion == null)
            {
                SelectedQuestion = QuestionList.First();
                return;
            }
            int currentIndex = QuestionList.IndexOf(SelectedQuestion);
            if (currentIndex < QuestionList.Count - 1)
            {
                SelectedQuestion = QuestionList[currentIndex + 1];
            }
            else
            {
                SelectedQuestion = QuestionList.First();
            }
        }

        [RelayCommand]
        private void FirstItem()
        {
            if (QuestionList == null)
                return;

            SelectedQuestion = QuestionList.FirstOrDefault();
        }

        [RelayCommand]
        private void LastItem()
        {
            if (QuestionList == null)
                return;

            SelectedQuestion = QuestionList.LastOrDefault();
        }
        #endregion

        #region Wording Commands
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
        #endregion

        [RelayCommand]
        private void ToggleQuestion(SurveyQuestionRecord question)
        {
            if (CurrentSurvey.Locked)
            {
                _dialogService.ShowMessage("Cannot modify locked surveys.");
                return;
            }

           
            if (question == null)
                return;

            if (question.Item.QuestionType == QuestionType.Heading || question.Item.QuestionType == QuestionType.Subheading)
            {
                return;
            }

            // if current q is standalone, make it a series using the previous whole qnum + a letter
            // if current q is in a series, make it a standalone by using the next qnum value, and increment each subsequent qnum by 1

            string newQnum = question.Item.Qnum;

            if (question.Item.QuestionType == QuestionType.Standalone)
            {
                // make it a series question
                var previousQuestion = QuestionList.Where(x => x.GetQnumValue() < question.Item.GetQnumValue()).LastOrDefault();

                if (previousQuestion == null)
                    return;

                var previousQnum = previousQuestion?.GetQnumValue() ?? -1;              

                if (previousQnum == -1)
                {
                    _dialogService.ShowError("Cannot convert to series question as there is no previous question to base the qnum on.", "Toggle Error");
                    return;
                }

                if (previousQuestion.QuestionType == QuestionType.Heading || previousQuestion.QuestionType == QuestionType.Subheading)
                    return;

                newQnum = previousQnum.ToString("000");

                if (previousQuestion.QuestionType == QuestionType.Series)
                {
                    var previousSuffix = previousQuestion.GetQnum().Substring(3, 1);
                    newQnum += (char)(previousSuffix[0] + 1);
                }
                else if (previousQuestion.QuestionType == QuestionType.Heading || previousQuestion.QuestionType == QuestionType.Subheading)
                {
                   // question.Item.Qnum = (previousQnum + 1).ToString("000") + "a";
                    
                }
                else
                {
                    newQnum += "b";
                }
            }
            else
            {
                // make it a standalone question
                var nextQnum = QuestionList.Where(x => x.GetQnumValue() > question.Item.GetQnumValue()).FirstOrDefault()?.GetQnumValue() ?? 0;
                if (nextQnum == 0)
                {
                    // no next question, so just increment current by 1
                    newQnum = (question.Item.GetQnumValue() + 1).ToString();
                }
                else
                {
                    newQnum = nextQnum.ToString();
                }
                
            }
            question.Item.Qnum = newQnum;
            // renumber the rest of the questions
            CurrentSurvey.Renumber(0);
            
        }

        // ask user to document, backup comments, confirm delete by typing 'DELETE' then remove from list and add to removed collection
        private void ProcessDeletes()
        {
            if (_dialogService.Confirm("Do you want to document these deletes?"))
            {
                //_dialogService.ShowDialog(new DocumentDeletesViewModel(_dialogService, Removed));
            }

            foreach (var question in Removed)
            {
                // transfer comments
                _commentService.BackupComments(question.ID);
                CurrentSurvey.RemoveQuestion(question, true);

                RecordList.Remove(RecordList.First(x=>x.Item == question));
            }
                
            Removed.Clear();
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
            bool? result = _dialogService.ShowDialog(wordingVM);

            if (result.Value) // if closed with OK
            {
                // apply wording to current question
                //SetResponse(type, wordingVM.CurrentWording);
            }
            else
            {
                //UpdateResponse(type);
            }
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
