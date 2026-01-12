using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmLib.ViewModels;
using ITCLib;
using ITC_Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using System.IO;

namespace SDIFrontEnd_WPF.ViewModels
{
    // TODO: scroll to bottom when adding response
    public partial class PraccingEntryViewModel : WorkspaceViewModel 
    {
        private readonly IWindowService _windowService;
        private readonly IPraccingService _praccingService;
        private readonly ISurveyService _surveyService;
        private readonly IPeopleService _peopleService;
        private readonly IFileDialogService _dialogService;

        public PraccingImagesViewModel ResponseImageViewModel { get; set; }

        // data lists
        [ObservableProperty]
        private ObservableCollection<Survey> surveyList = new();
        [ObservableProperty]
        private ObservableCollection<Person> peopleList = new();
        [ObservableProperty]
        private ObservableCollection<PraccingCategory> categoryList = new();
        // records
        [ObservableProperty]
        private ObservableCollection<PraccingIssueRecord> allRecords = new();

        [ObservableProperty]
        private ObservableCollection<PraccingIssueRecord> records = new();


        // current record properties
        [ObservableProperty]
        private PraccingIssueViewModel? issueVM;

        private PraccingIssueRecord? currentRecord;
        public PraccingIssueRecord? CurrentRecord { 
            get => currentRecord; 
            set
            {
                SaveRecord();
                if (SetProperty(ref currentRecord, value)) 
                    RefreshCurrentIssue();

                NextIssueCommand.NotifyCanExecuteChanged();
                PreviousIssueCommand.NotifyCanExecuteChanged();
                LastIssueCommand.NotifyCanExecuteChanged();
                FirstIssueCommand.NotifyCanExecuteChanged();
            } 
        }

        [ObservableProperty]
        private PraccingImage? currentImage;
       
        private PraccingResponseViewModel? currentResponse;
        public PraccingResponseViewModel? CurrentResponse
        {
            get => currentResponse;
            set
            {
                if (SetProperty(ref currentResponse, value))
                {
                    if (CurrentResponse != null)
                        ResponseImageViewModel.SetImages(new ObservableCollection<PraccingImage>(CurrentResponse?.Response.Images));
                    else
                        ResponseImageViewModel.SetImages(new ObservableCollection<PraccingImage>());
                }
            }
        }
        [ObservableProperty]
        private ObservableCollection<string> languages = new();

        private string dbImageRepo = @"\\psychfile\psych$\psych-lab-gfong\SMG\Praccing Images";


        private Survey? currentSurvey;
        public Survey? CurrentSurvey
        {
            get => currentSurvey;
            set
            {
                SetProperty(ref currentSurvey, value);
                if (value != null)
                {
                    // Load the issues for the selected survey

                    LoadSurveyIssues(value.SID);
                    _ = LoadLists();
                    LoadLanguages(value);
                    RefreshCurrentIssue();
                }
                NotifyAllCommandCanExecuteChanged();
            }
        }

        /// <summary>
        /// Used to move to specified record.
        /// </summary>
        private PraccingIssueRecord selectedRecord;
        public PraccingIssueRecord SelectedRecord
        {
            get => selectedRecord;
            set
            {
                if (SetProperty(ref selectedRecord, value))
                {
                    CurrentRecord = value;
                }
            }
        }

        [ObservableProperty]
        private PraccingIssue selectedIssue;

        public int TotalIssues => Records.Count();
        public int UnresolvedIssues => Records.Count(x => !x.Item.Resolved);

        
        public int CurrentIndex => Records.IndexOf(CurrentRecord) == -1 ? 0 : Records.IndexOf(CurrentRecord) + 1;

        public List<string> CategorySummaries
        {
            get
            {
                List<string> summaries = new List<string>();

                foreach (PraccingCategory pc in CategoryList)
                {
                    int categoryTotal = Records.Count(x => x.Item.Category.ID == pc.ID);
                    int unresolvedCategory = Records.Count(x => x.Item.Category.ID == pc.ID && !x.Item.Resolved);
                    summaries.Add($"{categoryTotal} {pc.Category} issues. {unresolvedCategory} unresolved.");
                }

                return summaries.OrderByDescending(x => int.Parse(x.Split(' ')[0])).ToList();
            }
        }

        [ObservableProperty]
        private bool isSearching;

        [ObservableProperty]
        private bool isSearchVisible;

        [ObservableProperty]
        private string filterString;

        [ObservableProperty]
        private bool isFiltered;

        public PraccingEntryViewModel(IWindowService windowService, IPraccingService praccingService, ISurveyService surveyService, IPeopleService peopleService, IFileDialogService dialogService)
        {
            _windowService = windowService;
            _praccingService = praccingService;
            _surveyService = surveyService;
            _peopleService = peopleService;
            _dialogService = dialogService;
            SurveyList = new ObservableCollection<Survey>(_surveyService.GetAllSurveys());
            // Register for the message
            WeakReferenceMessenger.Default.Register<IssueSelectedMessage>(this, (r, m) =>
            {
                if (m.Value.ID == 0)
                    AddIssue();
                else
                    CurrentRecord = Records.FirstOrDefault(x => x.Item.ID == m.Value.ID);

            });
            ResponseImageViewModel = new PraccingImagesViewModel();
        }

        partial void OnRecordsChanged(ObservableCollection<PraccingIssueRecord>? oldValue, ObservableCollection<PraccingIssueRecord> newValue)
        {
            OnPropertyChanged(nameof(TotalIssues));
        }

        #region Commands

        // TODO not working. trying to get survey list loaded using async
        [RelayCommand]
        private async Task OnLoadedAsync()
        {
            var surveys = await _surveyService.GetAllSurveysAsync();
            SurveyList = new ObservableCollection<Survey>(surveys);
        }

        [RelayCommand]
        private void LoadSurveyIssues(int survID)
        {
            AllRecords.Clear();
            var issues = _praccingService.GetPraccingIssues(survID);
            foreach (var issue in issues)
            {
                AllRecords.Add(new PraccingIssueRecord(issue));
            }
            if (AllRecords.Count == 0)
            {
                AddFirstIssue();
            }
            Records = AllRecords;
            CurrentRecord = Records.First();
            OnPropertyChanged(nameof(CurrentIndex));
            OnPropertyChanged(nameof(TotalIssues));
            OnPropertyChanged(nameof(UnresolvedIssues));
            OnPropertyChanged(nameof(CategorySummaries));
            BrowseIssuesCommand.NotifyCanExecuteChanged();
            
        }

        [RelayCommand]
        private void RefreshCurrentIssue()
        {
            if (CurrentRecord != null)
            {
                CurrentRecord.Item.Responses = CurrentRecord.Item.Responses.OrderBy(r => r.ResponseDate).ToList();
                IssueVM = new PraccingIssueViewModel(CurrentRecord);
                OnPropertyChanged(nameof(IssueVM));
                               
                if (CurrentRecord.Item.Responses.Count > 0)
                {
                    CurrentResponse = new PraccingResponseViewModel(CurrentRecord.Item.Responses.First());
                }
                else
                {
                    CurrentResponse = null;
                }
            }
            else
            {
                IssueVM = null;
            }
            OnPropertyChanged(nameof(CurrentIndex));
        }

        [RelayCommand(CanExecute = "CanSave")]
        private void SaveRecord()
        {
            if (CurrentRecord == null)
                return;
            
            if (CurrentRecord.NewRecord)
            {
                if (_praccingService.AddPraccingIssue(CurrentRecord.Item) == 1)
                {
                    // notify error
                }

                CurrentRecord.NewRecord = false;
                CurrentRecord.Dirty = false;
            }
            else if (CurrentRecord.Dirty)
            {
                if (_praccingService.UpdatePraccingIssue(CurrentRecord.Item) == 1)
                { 
                    // notify error
                }

                CurrentRecord.Dirty = false;
            }

            SaveResponses();

            SaveImages();

            OnPropertyChanged(nameof(CurrentRecord));
            IssueVM.UpdateStatus();
        }

        bool CanSave() => CurrentRecord != null;

        private int SaveResponses()
        {
            if (CurrentRecord == null)
                return 1;

            foreach (PraccingResponse response in CurrentRecord.AddedResponses)
            {
                response.IssueID = CurrentRecord.Item.ID;
                _praccingService.AddPraccingResponse(response);
            }
            CurrentRecord.AddedResponses.Clear();

            foreach (PraccingResponse response in CurrentRecord.EditedResponses)
            {
                _praccingService.UpdatePraccingResponse(response);
            }
            CurrentRecord.EditedResponses.Clear();

            foreach (PraccingResponse response in CurrentRecord.DeletedResponses)
            {
                _praccingService.DeletePraccingResponse(response);
            }
            CurrentRecord.DeletedResponses.Clear();
            return 0;
        }

        public int SaveImages()
        {
            if (CurrentRecord == null)
                return 1;

            foreach (PraccingImage img in CurrentRecord.AddedImages)
            {
                img.PraccID = CurrentRecord.Item.ID;
                _praccingService.AddPraccingImage(img);
            }
            CurrentRecord.AddedImages.Clear();

            foreach (PraccingImage img in CurrentRecord.DeletedImages)
            {
                _praccingService.DeletePraccingImage(img);
                try
                {
                    File.Delete(img.Path);
                }
                catch
                {

                }
            }
            CurrentRecord.DeletedImages.Clear();

            foreach (PraccingImage img in CurrentRecord.AddedResponseImages)
            {
                var response = CurrentRecord.Item.Responses.Where(x => x.Images.Contains(img)).FirstOrDefault();
                if (response == null) continue;
                img.PraccID = response.ID;
                _praccingService.AddResponseImage(img);
            }
            CurrentRecord.AddedResponseImages.Clear();

            foreach (PraccingImage img in CurrentRecord.DeletedResponseImages)
            {
                _praccingService.DeletePraccingResponseImage(img);
                try
                {
#if DEBUG

#else 
                    File.Delete(img.Path);
#endif
                }
                catch
                {

                }
            }
            CurrentRecord.DeletedResponseImages.Clear();
            return 0;
        }

        [RelayCommand(CanExecute = "CanRefresh")]
        private void AddIssue()
        {
            PraccingIssue newIssue = new PraccingIssue() { Survey = CurrentSurvey, IssueDate = DateTime.Today };
            PraccingIssueRecord newRecord = new PraccingIssueRecord(newIssue) { NewRecord = true };
            Records.Add(newRecord);
            CurrentRecord = newRecord;

            OnPropertyChanged(nameof(TotalIssues));
            OnPropertyChanged(nameof(UnresolvedIssues));
            OnPropertyChanged(nameof(CategorySummaries));
        }

        [RelayCommand(CanExecute = "CanRefresh")]
        private void DeleteIssue()
        {
            if (CurrentRecord != null)
            {
                if (MessageBox.Show("Are you sure you want to delete this issue?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;

                _praccingService.DeletePraccingIssue(CurrentRecord.Item);
                Records.Remove(CurrentRecord);
                if (CurrentRecord.NewRecord)
                    CurrentRecord.NewRecord = false;
                CurrentRecord = Records.FirstOrDefault();

                OnPropertyChanged(nameof(TotalIssues));
                OnPropertyChanged(nameof(UnresolvedIssues));
                OnPropertyChanged(nameof(CategorySummaries));
            }
        }

        [RelayCommand(CanExecute = "CanRefresh")]
        private void AddResponse() 
        {
            if (CurrentRecord == null) return;
            PraccingResponse newResponse = new PraccingResponse() { IssueID = CurrentRecord.Item.ID, ResponseDate = DateTime.Now };
            CurrentRecord.AddResponse(newResponse);
            PraccingResponseViewModel newResponseVM = new PraccingResponseViewModel(newResponse);
            IssueVM.ResponsesVM.Add(newResponseVM);

            CurrentResponse = newResponseVM;
            IssueVM.UpdateStatus();
        }

        [RelayCommand(CanExecute = "CanRefresh")]
        private void DeleteResponse(PraccingResponse response) 
        {
            if (CurrentResponse == null)
                return;

            if (CurrentRecord != null && response != null)
            {
                if (MessageBox.Show("Are you sure you want to delete this response?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;

                CurrentRecord.DeletedResponses.Add(response);
                CurrentRecord.Item.Responses.Remove(response);
                CurrentResponse.SetResponse(CurrentRecord.Item.Responses.FirstOrDefault());
                IssueVM.ResponsesVM.Remove(IssueVM.ResponsesVM.FirstOrDefault(x => x.Response.ID == response.ID));
                IssueVM.UpdateStatus();
            }
        }

        [RelayCommand (CanExecute ="CanMoveFirst")]
        private void FirstIssue()
        {
            if (Records.Count > 0)
                CurrentRecord = Records.First();
        }
        bool CanMoveFirst()
        {
            if (Records.Count > 0)
            {
                int index = Records.IndexOf(CurrentRecord);
                return index > 0;
            }
            return false;
        }
        [RelayCommand (CanExecute ="CanMoveLast")]
        private void LastIssue()
        {
            if (Records.Count > 0)
                CurrentRecord = Records.Last();
        }
        bool CanMoveLast()
        {
            if (Records.Count > 0)
            {
                int index = Records.IndexOf(CurrentRecord);
                return index < Records.Count - 1;
            }
            return false;
        }

        [RelayCommand (CanExecute ="CanMovePrevious")]
        private void PreviousIssue()
        {
            if (Records.Count > 0)
            {
                int index = Records.IndexOf(CurrentRecord);
                if (index > 0)
                    CurrentRecord = Records[index - 1];
            }
        }
        bool CanMovePrevious()
        {
            if (Records.Count > 0)
            {
                int index = Records.IndexOf(CurrentRecord);
                return index > 0;
            }
            return false;
        }
        [RelayCommand (CanExecute ="CanMoveNext")]
        private void NextIssue()
        {
            if (Records.Count > 0)
            {
                int index = Records.IndexOf(CurrentRecord);
                if (index < Records.Count - 1)
                    CurrentRecord = Records[index + 1];
            }
        }
        bool CanMoveNext()
        {
            if (Records.Count > 0)
            {
                int index = Records.IndexOf(CurrentRecord);
                return index < Records.Count - 1;
            }
            return false;
        }

        [RelayCommand (CanExecute ="CanBrowse")]
        private void BrowseIssues()
        {
            var issues = Records.Select(x => new PraccingIssue
            {
                ID = x.Item.ID,
                Survey = x.Item.Survey,
                VarNames = x.Item.VarNames,
                Category = x.Item.Category,
                IssueDate = x.Item.IssueDate,
                IssueFrom = x.Item.IssueFrom,
                IssueTo = x.Item.IssueTo,
                Description = x.Item.Description,
            }).ToList();
            _windowService.ShowSearchWindow(issues);
        }

        private bool CanBrowse()
        {
            if (Records.Count > 0)
                return true;
            else
                return false;
        }

        [RelayCommand(CanExecute = "CanRefresh")]
        private void AddImage()
        {
            if (CurrentRecord == null)
                return;

            // open file picker, take file name
            string? path = _dialogService.OpenImageFileDialog();

            if (path == null)
                return;


            string newFileName = "Praccing Image - " + DateTime.Now.Month.ToString().Trim() +
                        DateTime.Now.Day.ToString() + DateTime.Now.Year.ToString() + DateTime.Now.Hour.ToString() +
                        DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString() + ".png";

            // copy the image to the praccing images folder
            System.IO.File.Copy(path, this.dbImageRepo + @"\" + newFileName);

            // create new image 
            PraccingImage newImage = new PraccingImage()
            {
                FilePath = this.dbImageRepo + @"\" + newFileName,
                PraccID = CurrentRecord.Item.ID,
                Path = this.dbImageRepo + @"\" + newFileName,
            };
            CurrentRecord.Item.Images.Add(newImage);
            CurrentRecord.AddedImages.Add(newImage);
            IssueVM.ImagesVM.AddImage(newImage);
            IssueVM.UpdateStatus();
        }

        [RelayCommand(CanExecute = "CanRefresh")]
        private void AddResponseImage()
        {
            if (CurrentResponse == null)
                return;

            // open file picker, take file name
            string? path = _dialogService.OpenImageFileDialog();

            if (path == null)
                return;

            string newFileName = "Praccing Response Image - " + DateTime.Now.Month.ToString().Trim() +
                        DateTime.Now.Day.ToString() + DateTime.Now.Year.ToString() + DateTime.Now.Hour.ToString() +
                        DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString() + ".png";

            // copy the image to the praccing images folder
            System.IO.File.Copy(path, dbImageRepo + @"\" + newFileName);

            // create new image 
            PraccingImage newImage = new PraccingImage()
            {
                FilePath = dbImageRepo + @"\" + newFileName,
                PraccID = CurrentResponse.Response.ID,
                Path = this.dbImageRepo + @"\" + newFileName,
            };
            CurrentResponse.Response.Images.Add(newImage);
            CurrentRecord.AddedResponseImages.Add(newImage);
            ResponseImageViewModel.AddImage(newImage);
            OnPropertyChanged(nameof(ResponseImageViewModel.TotalImages));
            OnPropertyChanged(nameof(ResponseImageViewModel));
            IssueVM.UpdateStatus();
        }

        [RelayCommand(CanExecute = "CanRefresh")]
        private void DeleteImage(PraccingImage image)
        {
            if (image != null)
            {
                if (MessageBox.Show("Are you sure you want to delete this image?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
                
                CurrentRecord.Item.Images.Remove(image);
                CurrentRecord.DeletedImages.Add(image);
                IssueVM.ImagesVM.RemoveImage(image);
                IssueVM.UpdateStatus();
            }
        }

        [RelayCommand(CanExecute = "CanRefresh")]
        private void DeleteResponseImage(PraccingImage image)
        {
            if (image != null)
            {
                if (MessageBox.Show("Are you sure you want to delete this image?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
                
                CurrentResponse.Response.Images.Remove(image);
                CurrentRecord.DeletedResponseImages.Add(image);
                ResponseImageViewModel.SetImages(new ObservableCollection<PraccingImage>(CurrentResponse.Response.Images));
                IssueVM.UpdateStatus();
            }
        }

        [RelayCommand(CanExecute = "CanRefresh")]
        private void FilterUnresolved(bool filter)
        {
            if (filter)
            {
                if (AllRecords.Count > 0)
                {
                    var unresolved = AllRecords.Where(x => !x.Item.Resolved).ToList();
                    Records = [.. unresolved];
                }
            }
            else
            {
                Records = AllRecords;
            }
            CurrentRecord = Records.FirstOrDefault();
            RefreshCurrentIssue();
        }

        [RelayCommand(CanExecute = "CanRefresh")]
        private void FilterIssues(string description)
        {
            if (description == null || description.Trim() == string.Empty)
                return;
            
            if (Records.Count > 0)
            {
                var filtered = Records.Where(x => x.Item.Description.Contains(description)).ToList();
                Records = [.. filtered];
            }
            CurrentRecord = Records.FirstOrDefault();
            RefreshCurrentIssue();
            IsSearchVisible = false;
        }

        [RelayCommand (CanExecute ="CanRefresh")]
        private void TrySearching()
        {
            if (!IsSearching )
            {
                Records = AllRecords;
                CurrentRecord = Records.FirstOrDefault();
                RefreshCurrentIssue();
                IsSearchVisible = false;
                return;
            }

            IsSearchVisible = true;
        }

        [RelayCommand (CanExecute ="CanRefresh")]
        void Refresh()
        {
            if (CurrentSurvey == null)
                return;

            SaveRecord();

            LoadSurveyIssues(CurrentSurvey.SID);

            if (IsSearching)
            {
                FilterIssues(FilterString);
            }else if (IsFiltered)
            {
                // if filtered, we need to reapply the filter
                FilterUnresolved(IsFiltered);
            }
            else
            {
                Records = AllRecords;
            }
            
            CurrentRecord = Records.FirstOrDefault();
            RefreshCurrentIssue();

        }
        bool CanRefresh() => CurrentSurvey != null;

        #endregion

        private void NotifyAllCommandCanExecuteChanged()
        {
            RefreshCommand.NotifyCanExecuteChanged();
            NextIssueCommand.NotifyCanExecuteChanged();
            PreviousIssueCommand.NotifyCanExecuteChanged();
            LastIssueCommand.NotifyCanExecuteChanged();
            FirstIssueCommand.NotifyCanExecuteChanged();
            SaveRecordCommand.NotifyCanExecuteChanged();
            FilterUnresolvedCommand.NotifyCanExecuteChanged();
            FilterIssuesCommand.NotifyCanExecuteChanged();
            TrySearchingCommand.NotifyCanExecuteChanged();
            AddIssueCommand.NotifyCanExecuteChanged();
            DeleteIssueCommand.NotifyCanExecuteChanged();
            AddResponseCommand.NotifyCanExecuteChanged();
            DeleteResponseCommand.NotifyCanExecuteChanged();
            AddImageCommand.NotifyCanExecuteChanged();
            AddResponseImageCommand.NotifyCanExecuteChanged();
            DeleteImageCommand.NotifyCanExecuteChanged();
            DeleteResponseImageCommand.NotifyCanExecuteChanged();
        }

        private async Task LoadLists()
        {
            var people = await _peopleService.GetPeopleBasicsAsync();
            PeopleList = new ObservableCollection<Person>(people);
            var categories = await _praccingService.GetPraccingCategories();
            CategoryList = new ObservableCollection<PraccingCategory>(categories);
            OnPropertyChanged(nameof(CategorySummaries));
        }

        private void LoadLanguages(Survey survey)
        {
            Languages = [.. survey.ListLanguages];
            if (!Languages.Any(x => x.Equals("English", StringComparison.OrdinalIgnoreCase)))
            {
                Languages.Insert(0, "English");
            }
        }

        private void AddFirstIssue()
        {
            PraccingIssue newIssue = new PraccingIssue() { Survey = CurrentSurvey, IssueDate = DateTime.Today };
            PraccingIssueRecord newRecord = new PraccingIssueRecord(newIssue) { NewRecord = true };
            AllRecords.Add(newRecord);
            CurrentRecord = newRecord;
        }

        protected override void OnDispose()
        {
            // TODO unhook events
            //foreach (var record in Records)
            //{
            //    record.Dispose();
            //}
        }

    }
}
