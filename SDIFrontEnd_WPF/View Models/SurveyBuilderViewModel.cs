using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITCLib;
using MvvmLib.ViewModels;
using SDIFrontEnd_WPF.ViewModels;
using System.Collections.ObjectModel;

namespace SDIFrontEnd_WPF
{
    public partial class SurveyBuilderViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IApiSurveyService _surveyService;
        private readonly IApiQuestionService _questionService;
        private readonly ReferenceDataStore _referenceDataService;
        private readonly IApiWordingService _wordingService;
        private readonly IApiCommentService _commentService;
        private readonly IApiPeopleService _peopleService;
        private readonly IApiVarNameService _varnameService;
        private readonly WordingData _wordingData;

        private readonly Survey CurrentSurvey;
        private readonly ObservableCollection<SurveyQuestionRecord> _recordList;
        public ObservableCollection<SurveyQuestionRecord> RecordList => _recordList;

        public ObservableCollection<SurveyQuestion> QuestionList => new ObservableCollection<SurveyQuestion>(RecordList.Select(r => r.Item));

        public List<VarNameLabel> TopicLabels { get; set; }
        public List<VarNameLabel> ContentLabels { get; set; }
        public List<VarNameLabel> DomainLabels { get; set; }
        public List<VarNameLabel> ProductLabels { get; set; }

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

        [ObservableProperty]
        private SurveyQuestion? goToVar;

        public ObservableCollection<QuestionComment> Comments => new ObservableCollection<QuestionComment>(SelectedQuestion?.Comments ?? new List<QuestionComment>());

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ImageIndex))]
        private SurveyImage? currentImage;

        public string? ImageIndex => $"{(SelectedQuestion?.Images.IndexOf(CurrentImage) + 1)} of {SelectedQuestion?.Images.Count}";
        public string? ItemPosition => $"{(QuestionList.IndexOf(SelectedQuestion) + 1)} of {QuestionList?.Count}";



        public string CurrentQuestionText => SelectedQuestion?.GetQuestionTextHTML() ?? string.Empty;

        //bool Editable => !(CurrentSurvey == null || CurrentSurvey.Locked);
        bool Editable => !(CurrentSurvey == null);
        public bool Locked => !Editable;


        // other windows
        public TranslationViewModel? TranslationVM;
        public RelatedQuestionsViewModel? RelatedQsVM;

        public string StatusSummary => $"Total Questions: {QuestionList.Count}, Added: {Added.Count}, Modified: {RecordList.Count(x=>x.Dirty || x.DirtyLabels || x.DirtyQnum)}, Removed: {Removed.Count}";

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

        public SurveyBuilderViewModel(IDialogService dialogService, IApiSurveyService surveyService, IApiQuestionService questionService, ReferenceDataStore referenceData, IApiWordingService wordingService,
            IApiPeopleService peopleService, IApiCommentService commentService, IApiVarNameService varnameService, WordingData wordingData, Survey survey)
        {
            _dialogService = dialogService;
            _surveyService = surveyService ?? throw new ArgumentNullException(nameof(surveyService), "Survey service cannot be null.");
            _questionService = questionService ?? throw new ArgumentNullException(nameof(questionService), "Question service cannot be null.");
            _referenceDataService = referenceData ?? throw new ArgumentNullException(nameof(referenceData), "Reference data service cannot be null.");
            _wordingService = wordingService ?? throw new ArgumentNullException(nameof(wordingService), "Wording service cannot be null.");
            _peopleService = peopleService ?? throw new ArgumentNullException(nameof(peopleService), "People service cannot be null.");
            _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService), "Comment service cannot be null.");
            _varnameService = varnameService ?? throw new ArgumentNullException(nameof(varnameService), " VarName service cannot be null.");
            if (survey == null) throw new ArgumentNullException(nameof(survey), "Questions cannot be null");

            CurrentSurvey = survey;
            OnPropertyChanged(nameof(Editable));
            _recordList = new ObservableCollection<SurveyQuestionRecord>(survey.Questions.Select(x => new SurveyQuestionRecord(x)));

            TopicLabels = _referenceDataService.TopicLabels.ToList() ?? new List<VarNameLabel>();
            ContentLabels = _referenceDataService.ContentLabels.ToList() ?? new List<VarNameLabel>();
            DomainLabels = _referenceDataService.DomainLabels.ToList() ?? new List<VarNameLabel>();
            ProductLabels = _referenceDataService.ProductLabels.ToList() ?? new List<VarNameLabel>();
            _wordingData = wordingData ?? throw new ArgumentNullException(nameof(wordingData), "Wording data cannot be null.");
            SelectedQuestionRecord = _recordList.FirstOrDefault() ?? new SurveyQuestionRecord(new SurveyQuestion("Default", "0000"));
            SelectedQuestionRecords = new ObservableCollection<SurveyQuestionRecord>();
            SelectedQuestions = new ObservableCollection<SurveyQuestion>();

            DropHandler = new QuestionRecordDropHandler(OnItemsReordered);
        }

        private void OnItemsReordered()
        {
            ReorderCommand.Execute(null);
        }

        void ModifyPreP(Wording prep, SurveyQuestion question)
        {
            question.PrePW = prep;
            SurveyQuestionRecord record = new SurveyQuestionRecord(question);
            if (!Modified.Contains(question)) Modified.Add(question);
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        #region Property Changed Events
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
                _ = UpdateRelatedQuestions(question.VarName.RefVarName);
            }

        }


        partial void OnGoToVarChanged(SurveyQuestion? value)
        {
            if (value == null)
                return;

            if (SelectedQuestionRecord == null)
                return;

            var record = _recordList.FirstOrDefault(r => r.Item == value);
            if (record != null && record.Item.ID != SelectedQuestionRecord.Item.ID)
                SelectedQuestionRecord = record;
        }

        

        partial void OnSelectedQuestionChanged(SurveyQuestion? value)
        {
            if (value == null)
                return;
            var record = _recordList.FirstOrDefault(r => r.Item == value);
            if (record != null && record.Item.ID != SelectedQuestionRecord.Item.ID)
                SelectedQuestionRecord = record;
        }

        partial void OnPrePIDChanged(int oldValue, int newValue)
        {
            if (SelectedQuestion?.PrePW.WordID == newValue)
                return;

            Wording? found = _wordingData.PreP.FirstOrDefault(x => x.WordID == newValue);
            if (found != null)
            {
                SelectedQuestion.PrePW = found;
                if (!Modified.Contains(SelectedQuestion)) Modified.Add(SelectedQuestion);
            }
            else
                PrePID = oldValue;
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnPreIIDChanged(int oldValue, int newValue)
        {
            if (SelectedQuestion?.PreIW.WordID == newValue)
                return;

            Wording? found = _wordingData.PreI.FirstOrDefault(x => x.WordID == newValue);
            if (found != null)
            {
                SelectedQuestion.PreIW = found;
                if (!Modified.Contains(SelectedQuestion)) Modified.Add(SelectedQuestion);
            }
            else
                PreIID = oldValue;
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnPreAIDChanged(int oldValue, int newValue)
        {
            if (SelectedQuestion?.PreAW.WordID == newValue)
                return;

            Wording? found = _wordingData.PreA.FirstOrDefault(x => x.WordID == newValue);
            if (found != null)
            {
                SelectedQuestion.PreAW = found;
                if (!Modified.Contains(SelectedQuestion)) Modified.Add(SelectedQuestion);
            }
            else
                PreAID = oldValue;
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnLitQIDChanged(int oldValue, int newValue)
        {
            if (SelectedQuestion?.LitQW.WordID == newValue)
                return;
            Wording? found = _wordingData.LitQ.FirstOrDefault(x => x.WordID == newValue);
            if (found != null)
            {
                SelectedQuestion.LitQW = found;
                if (!Modified.Contains(SelectedQuestion)) Modified.Add(SelectedQuestion);
            }
            else
                LitQID = oldValue;
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnPstIIDChanged(int oldValue, int newValue)
        {
            if (SelectedQuestion?.PstIW.WordID == newValue)
                return;
            Wording? found = _wordingData.PstI.FirstOrDefault(x => x.WordID == newValue);
            if (found != null)
            {
                SelectedQuestion.PstIW = found;
                if (!Modified.Contains(SelectedQuestion)) Modified.Add(SelectedQuestion);
            }
            else
                PstIID = oldValue;
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnPstPIDChanged(int oldValue, int newValue)
        {
            if (SelectedQuestion?.PstPW.WordID == newValue)
                return;
            Wording? found = _wordingData.PstP.FirstOrDefault(x => x.WordID == newValue);
            if (found != null)
            {
                SelectedQuestion.PstPW = found;
                if (!Modified.Contains(SelectedQuestion)) Modified.Add(SelectedQuestion);
            }
            else
                PstPID = oldValue;
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        partial void OnRespNameChanged(string? oldValue, string? newValue)
        {
            if (SelectedQuestion == null)
                return;
            if (SelectedQuestion?.RespOptionsS.RespSetName == newValue)
                return;

            ResponseSet? found = _wordingData.RO.FirstOrDefault(x => x.RespSetName == newValue);
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

            ResponseSet? found = _wordingData.NR.FirstOrDefault(x => x.RespSetName == newValue);
            if (found != null)
            {
                SelectedQuestion.NRCodesS = found;
                if (!Modified.Contains(SelectedQuestion)) Modified.Add(SelectedQuestion);
            }
            else
                NRName = oldValue;

            OnPropertyChanged(nameof(CurrentQuestionText));
        }
        #endregion

        [RelayCommand]
        private async Task AddComment()
        {
            if (SelectedQuestion == null)
            {
                _dialogService.ShowError("No question selected for comments.", "Comments Error");
                return;
            }

            QuickCommentEntryViewModel vm = new QuickCommentEntryViewModel(_peopleService, _referenceDataService);
            await vm.LoadLists();
            bool? result = _dialogService.ShowDialog(vm);
            if (result.Value)
            {
                QuestionComment newComment = new QuestionComment(vm.NewComment)
                {
                    Survey = SelectedQuestion.SurveyCode,
                    VarName = SelectedQuestion.VarName.VarName,
                };

                if (!SelectedQuestionRecord.NewRecord)
                {
                if (await _commentService.AddCommentAsync(newComment) == 0)
                {
                    _dialogService.ShowError("Error saving comment.", "Comments Error");
                    return;
                }
                }

                SelectedQuestion.Comments.Add(newComment);
                OnPropertyChanged(nameof(Comments));
                OnPropertyChanged(nameof(StatusSummary));
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
            TranslationVM = new TranslationViewModel(_questionService, SelectedQuestion);
            _dialogService.ShowWindow(TranslationVM);
        }

        [RelayCommand(CanExecute = nameof(Editable))]
        private void AddTranslation()
        {
            if (SelectedQuestion == null)
            {
                _dialogService.ShowError("No question selected.", "Translation Error");
                return;
            }

            if (SelectedQuestion.ID == 0)
            {
                _dialogService.ShowError("Please save the question before adding translations.", "Translation Error");
                return;
            }

            // show translation window
            TranslationVM = new TranslationViewModel(_questionService, SelectedQuestion);
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

        [RelayCommand(CanExecute = nameof(Editable))]
        private async Task AddSurveyQuestion()
        {
            // enter VarName
            // if exists, ask user if they want to copy wordings or labels
            string newVarName = _dialogService.PromptForText("Enter VarName", "New Survey Question");

            if (string.IsNullOrWhiteSpace(newVarName))
                return;

            var newVarNameCC = Utilities.ChangeCC(newVarName, CurrentSurvey.CountryCode);
            if (CurrentSurvey.Questions.Any(x => x.VarName.VarName == newVarNameCC))
            {
                _dialogService.ShowMessage("This varname already exists in this survey.");
                return;
            }

            var existingQuestions = await _surveyService.FindQuestionsByRefVarName(newVarName);
            SurveyQuestion selectedSource = null;
            string newQnum = SelectedQuestion.Qnum.Substring(0,3);
            int position = QuestionList.IndexOf(SelectedQuestion);
            if (existingQuestions.Count > 0)
            {
                selectedSource = _dialogService.PickQuestion(existingQuestions);

                SurveyQuestion newQuestion;

                if (selectedSource == null)
                {
                    return;
                }
                else
                {
                    newQuestion = new SurveyQuestion(selectedSource.VarName.VarName);
                    newQuestion.SurveyCode = CurrentSurvey.SurveyCode;
                    newQuestion.VarName = selectedSource.VarName;
                    newQuestion.VarName.VarName = newVarNameCC;
                }
                newQuestion.Qnum = newQnum;
                CurrentSurvey.AddQuestion(newQuestion, position, true);
                Added.Add(newQuestion);
                RecordList.Insert(position, new SurveyQuestionRecord(newQuestion) {  NewRecord = true });
            }
            else
            {
                SurveyQuestion newQuestion = new SurveyQuestion(newVarName);
                newQuestion.VarName.VarName = Utilities.ChangeCC(newVarName, CurrentSurvey.CountryCode);
                newQuestion.SurveyCode = CurrentSurvey.SurveyCode;
                newQuestion.Qnum = newQnum;
                CurrentSurvey.AddQuestion(newQuestion, position, true);
                Added.Add(newQuestion);
                RecordList.Insert(position, new SurveyQuestionRecord(newQuestion) {  NewRecord = true });
            }
            OnPropertyChanged(nameof(RecordList));
        }

        [RelayCommand(CanExecute = nameof(Editable))]
        private void AddSeries()
        {
            if (SelectedQuestion.QnumSuffix != string.Empty)
            {
                _dialogService.ShowError("Series can only be added after standalone questions.", "Add Series Error");
                return;
            }

            // open SeriesBuilder viewmodel in new window
            // get list of questions to add 
            SeriesBuilderViewModel vm = new SeriesBuilderViewModel(_referenceDataService, _wordingData);
            vm.Load();
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

        [RelayCommand(CanExecute = nameof(Editable))]
        private void RemoveSurveyQuestion()
        {
            if (SelectedQuestionRecords == null) return;
            foreach (SurveyQuestionRecord q in SelectedQuestionRecords)
            {
                Removed.Add(q.Item);
                q.Deleted = true;
            }
        }

        [RelayCommand(CanExecute = nameof(Editable))]
        private async Task SaveChanges()
        {

            // TODO replace with SurveyManagerService.
            bool relock = false;

            // if the survey is locked, confirm with the user that they want to save changes, as this may overwrite fielded data.
            // If they confirm, unlock the survey, save changes, then relock the survey. If unlocking fails, do not save changes.
            if (CurrentSurvey.Locked)
            {
                if (!_dialogService.Confirm("You are saving changes to a locked survey. These changes may not represent what fielded. Are you sure you want to save these changes?"))
                    return;

                bool unlocked = await _surveyService.UnlockSurvey(CurrentSurvey.SID);
                if (unlocked)
                {
                    CurrentSurvey.Locked = false;
                    relock = true;
                }
                else
                {
                    _dialogService.ShowMessage("Failed to unlock survey. Changes have not been saved.");
                    return;
                }
            }


            // confirm deletions by making the user type DELETE
            if (this.Removed.Count > 0)
                if (_dialogService.PromptForText("One or more questions are being deleted. Type 'DELETE' to confirm.", "Confirm Deletes") != "DELETE")
                {
                    _dialogService.ShowMessage("Failed to confirm deletes. Changes have not been saved.");
                    return;
                }
                else
                {
                    // document deletions
                    await ProcessDeletes();
                }

            var deleted = new List<SurveyQuestionRecord>(RecordList.Where(x => x.Deleted));
            foreach (var r in deleted)
            {
                if (r.NewRecord)
                {
                    // if the record is new and deleted, just remove it from the list
                    CurrentSurvey.RemoveQuestion(r.Item, true);
                    RecordList.Remove(r);
                    Removed.Remove(r.Item);
                }else
                {
                    var result = await _questionService.DeleteQuestion(r.Item);
                    if (result)
                    {
                        CurrentSurvey.RemoveQuestion(r.Item, true);
                        RecordList.Remove(RecordList.First(x => x.Item == r.Item));
                        Removed.Remove(r.Item);
                    }
                }
            }

            
            var modified = new List<SurveyQuestionRecord>(RecordList.Where(x => x.ShouldSave || x.NewRecord));
            List<SurveyQuestion> qnumUpdates = new List<SurveyQuestion>();
            foreach (var r in modified)
            {
                
                if (r.NewRecord)
                {
                    var result = await _questionService.AddQuestion(r.Item);
                    if (result>0)
                    {
                        r.NewRecord= false ;
                        Added.Remove(r.Item);
                    }
                }
                
                if (r.DirtyWordings)
                {

                    var result = await _questionService.UpdateQuestion(r.Item);
                    if (result > 0)  r.DirtyWordings = false;
                }
                
                if (r.DirtyLabels)
                {
                    var result = await _varnameService.UpdateVariable(r.Item.VarName);
                    if (result) r.DirtyLabels = false;
                }
                
                if (r.DirtyQnum)
                {
                    qnumUpdates.Add(r.Item);
                }
               
            }

            if (qnumUpdates.Count()>0)
            {
               var result =  await _questionService.UpdateQnums(qnumUpdates);
                if (result)
                {
                    RecordList.Where(x => x.DirtyQnum).ToList().ForEach(x => x.DirtyQnum = false);
                }
            }

            OnPropertyChanged(nameof(RecordList));
            OnPropertyChanged(nameof(StatusSummary));
            OnPropertyChanged(nameof(Editable));
            
            // check if we unlocked this survey for saving, if so, relock it
            if (!relock) return;
            CurrentSurvey.Locked = await _surveyService.LockSurvey(CurrentSurvey.SID);
        }

        [RelayCommand]
        private async Task ViewRelatedQuestions()
        {
            if (SelectedQuestion == null)
            {
                _dialogService.ShowError("No question selected.", "Related Questions Error");
                return;
            }
            var relatedQuestions = await _surveyService.FindQuestionsByRefVarName(SelectedQuestion.VarName.RefVarName);
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
        private async Task ViewDeletedVars()
        {
            var deletes = await _surveyService.GetDeletedQuestions(CurrentSurvey.SID);
            // show related questions window
            DeletedQuestionsViewModel deletedVM = new DeletedQuestionsViewModel(deletes);
            _dialogService.ShowWindow(deletedVM);
        }

        [RelayCommand(CanExecute = nameof(Editable))]
        private void CopyPreviousWordings()
        {
            if (SelectedQuestion == null) return;

            // get previous question in survey
            var previousQuestion = QuestionList.Where(x => x.GetQnumValue() < SelectedQuestion.GetQnumValue()).LastOrDefault();

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
        private void NextImage()
        {
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
        private async Task OpenPreP(int wordID)
        {
            await OpenWordings("PreP", wordID);
        }

        [RelayCommand]
        private async Task OpenPreI(int wordID)
        {
            await OpenWordings("PreI", wordID);
        }

        [RelayCommand]
        private async Task OpenPreA(int wordID)
        {
            await OpenWordings("PreA", wordID);
        }

        [RelayCommand]
        private async Task OpenLitQ(int wordID)
        {
            await OpenWordings("LitQ", wordID);
        }

        [RelayCommand]
        private async Task OpenPstI(int wordID)
        {
            await OpenWordings("PstI", wordID);
        }

        [RelayCommand]
        private async Task OpenPstP(int wordID)
        {
            await OpenWordings("PstP", wordID);
        }

        [RelayCommand]
        private async Task OpenRespOptions(string respSetName)
        {
            OpenResponses("RespOptions", respSetName);
        }

        [RelayCommand]
        private async Task OpenNRCodes(string respSetName)
        {
            OpenResponses("NRCodes", respSetName);
        }
        #endregion

        [RelayCommand(CanExecute = nameof(Editable))]
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

        /// <summary>
        /// Add an image to the selected question. Images must be already stored in the correct network location.
        /// </summary>
        [RelayCommand(CanExecute = nameof(Editable))]

        private void AddImage()
        {
            if (SelectedQuestionRecord == null)
                return;

            string file = _dialogService.OpenSurveyImageFile();

            SurveyImage image = new SurveyImage(file.Substring(file.LastIndexOf(@"\") + 1));
            image.FilePath = file;
            image.QID = SelectedQuestion.ID;
            image.Survey = CurrentSurvey.SurveyCode;
            image.VarName = SelectedQuestion.VarName.VarName;

            image.ImageName = file.Substring(file.LastIndexOf(@"\") + 1);
            image.ImagePath = file;

            SelectedQuestionRecord.AddedImages.Add(image);
            SelectedQuestion.Images.Add(image);
            CurrentImage = SelectedQuestion.Images.LastOrDefault();
            OnPropertyChanged(nameof(ImageIndex));
            OnPropertyChanged(nameof(SelectedQuestionRecord));

        }

        [RelayCommand(CanExecute = nameof(Editable))]
        private void DeleteImage()
        {
            if (CurrentImage != null)
            {
                if (_dialogService.Confirm("Are you sure you want to delete this image?", "Confirm"))
                    return;

                SelectedQuestion.Images.Remove(CurrentImage);
                SelectedQuestionRecord.DeletedImages.Add(CurrentImage);
                CurrentImage = SelectedQuestion.Images.FirstOrDefault();
                OnPropertyChanged(nameof(ImageIndex));
            }
        }

        [RelayCommand]
        private void Reorder()
        {
            var list = RecordList;
            int start = CurrentSurvey.Questions[0].GetQnumValue() -1;
            Survey temp = new Survey();
            temp.Questions.AddRange(RecordList.Select(x => x.Item));
            temp.Renumber(start);
            foreach (var r in RecordList)
            {
                if (r.DirtyQnum)
                {
       
                }
            }
        }

        // ask user to document deletes with a comment, then save comments
        private async Task ProcessDeletes()
        {
            if (_dialogService.Confirm("Do you want to document these deletes?"))
            {
                QuickCommentEntryViewModel vm = new QuickCommentEntryViewModel(_peopleService, _referenceDataService);
                await vm.LoadLists();

                bool? result = _dialogService.ShowDialog(vm);

                if (!result.HasValue) return;

                if (!result.Value) return;

                foreach (SurveyQuestion q in Removed)
                {
                    DeletedComment newComment = new DeletedComment(vm.NewComment)
                    {
                        SurvID = CurrentSurvey.SID,
                        Survey = q.SurveyCode,
                        VarName = q.VarName.VarName,
                    };

                    if (!await _commentService.InsertDeletedComment(newComment))
                    {
                        _dialogService.ShowError("Error saving comment.", "Comments Error");
                    }
                }
            }
        }

        async Task UpdateRelatedQuestions(string refVarName)
        {
            var relatedQuestions = await _surveyService.FindQuestionsByRefVarName(SelectedQuestion.VarName.RefVarName);
            relatedQuestions.RemoveAll(q => q.ID == SelectedQuestion.ID); // remove current question from list

            RelatedQsVM.UpdateQuestions(relatedQuestions, CurrentSurvey.SurveyCodePrefix);
            OnPropertyChanged(nameof(RelatedQsVM));
        }

        private async Task OpenWordings(string type, int wordID)
        {
            WordingViewModel wordingVM = new WordingViewModel(_wordingData, _wordingService, _dialogService, type, wordID);
            await wordingVM.Load();
            bool? result = _dialogService.ShowDialog(wordingVM);

            if (result.Value) // if closed with OK
            {
                // apply wording to current question
                SetWording(type, wordingVM.CurrentItem.Wording);
            }
            else
            {
                UpdateWording(type);
            }
        }


        private async Task OpenResponses(string type, string setname)
        {
            ResponseSetViewModel wordingVM = new ResponseSetViewModel(_wordingData, _wordingService, _dialogService, type, setname);
            await wordingVM.Load();
            bool? result = _dialogService.ShowDialog(wordingVM);

            if (result.Value) // if closed with OK
            {
                // apply wording to current question
                SetResponse(type, wordingVM.CurrentItem.ResponseSet);
            }
            else
            {
                UpdateResponse(type);
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
                    SelectedQuestion.PrePW = _wordingData.PreP.FirstOrDefault(x => x.WordID == SelectedQuestion.PrePW.WordID);
                    break;
                case "PreI":
                    SelectedQuestion.PreIW = _wordingData.PreI.FirstOrDefault(x => x.WordID == SelectedQuestion.PreIW.WordID);
                    break;
                case "PreA":
                    SelectedQuestion.PreAW = _wordingData.PreA.FirstOrDefault(x => x.WordID == SelectedQuestion.PreAW.WordID);
                    break;
                case "LitQ":
                    SelectedQuestion.LitQW = _wordingData.LitQ.FirstOrDefault(x => x.WordID == SelectedQuestion.LitQW.WordID);
                    break;
                case "PstI":
                    SelectedQuestion.PstIW = _wordingData.PstI.FirstOrDefault(x => x.WordID == SelectedQuestion.PstIW.WordID);
                    break;
                case "PstP":
                    SelectedQuestion.PstPW = _wordingData.PstP.FirstOrDefault(x => x.WordID == SelectedQuestion.PstPW.WordID);
                    break;
                default:
                    break;
            }
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        private void SetResponse(string type, ResponseSet? responseSet)
        {
            if (responseSet == null || SelectedQuestion == null)
                return;
            switch (type)
            {
                case "RespOptions":
                    SelectedQuestion.RespOptionsS = responseSet;
                    RespName = responseSet.RespSetName;
                    break;
                case "NRCodes":
                    SelectedQuestion.NRCodesS = responseSet;
                    NRName = responseSet.RespSetName;
                    break;
                default:
                    break;
            }
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

        private void UpdateResponse(string type)
        {
            if (SelectedQuestion == null)
                return;
            switch (type)
            {
                case "RespOptions":
                    SelectedQuestion.RespOptionsS = _wordingData.RO.FirstOrDefault(x => x.RespSetName == SelectedQuestion.RespOptionsS.RespSetName);
                    break;
                case "NRCodes":
                    SelectedQuestion.NRCodesS = _wordingData.NR.FirstOrDefault(x => x.RespSetName == SelectedQuestion.NRCodesS.RespSetName);
                    break;
                default:
                    break;
            }
            OnPropertyChanged(nameof(CurrentQuestionText));
        }

       
    }
}
