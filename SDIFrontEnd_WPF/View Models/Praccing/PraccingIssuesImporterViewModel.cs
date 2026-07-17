using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Math;
using ITCLib;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;


using System.Text;
using System.Threading.Tasks;
namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class PraccingIssuesImporterViewModel : ViewModelBase
    {
        private readonly IApiPraccingService _praccingService;
        private readonly IDialogService _dialogService;
        private readonly IApiPeopleService _peopleService;

        [ObservableProperty]
        private Survey? selectedSurvey;

        [ObservableProperty]
        private string filePath = string.Empty;

        public ObservableCollection<Survey> SurveyList { get; private set; } = [];

        [ObservableProperty]
        private ObservableCollection<Person> peopleList;

        [ObservableProperty]
        private ObservableCollection<PraccingCategory> categoryList;
        public ObservableCollection<PraccingImportItemViewModel> ImportItems { get; private set; } = [];
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ItemPosition))]
        private PraccingImportItemViewModel? currentItem;

        public List<PraccingIssue> ReferenceList { get; set; } = new List<PraccingIssue>();
            
        string DBImageRepo = @"\\psychfile\psych$\psych-lab-gfong\SMG\Praccing Images";

        public string? ItemPosition => $"{(ImportItems.IndexOf(CurrentItem) + 1)} of {ImportItems?.Count}";

        [ObservableProperty]
        private bool loaded = false;

        public PraccingIssuesImporterViewModel(IApiPraccingService praccingService, IDialogService dialogService, IApiPeopleService peopleService)
        {
            _praccingService = praccingService;
            _dialogService = dialogService;
            _peopleService = peopleService;
        }

        public async Task LoadAsync()
        {
            SurveyList = new ObservableCollection<Survey>(await _praccingService.GetPraccingSurveys());
            

            PeopleList = new ObservableCollection<Person>(await _peopleService.GetPeopleBasics());

            CategoryList = new ObservableCollection<PraccingCategory>(await _praccingService.GetPraccingCategories());

            CurrentItem = null;
        }

        [RelayCommand]
        private void BrowseFiles()
        {
            string filename = _dialogService.OpenFile("DOC files (*.doc)|*.docx|All files (*.*)|*.*");
            FilePath = filename;
        }

        [RelayCommand]
        private async Task LoadFile()
        {
            // Parse file

            ImportItems.Clear();

            if (SelectedSurvey == null)
            {
                _dialogService.ShowMessage("Choose a survey before continuing.");
                return;
            }

            if (string.IsNullOrEmpty(FilePath))
            {
                _dialogService.ShowMessage("Choose a file before continuing.");
                return;
            }

            try
            {
                Survey survey = SelectedSurvey;
                PraccingIssueImporter importer = new PraccingIssueImporter(survey, FilePath);
                importer.PeopleList = await _peopleService.GetPeopleBasics();
                importer.CategoryList = await _praccingService.GetPraccingCategories();
                importer.ImportData();

                var images = importer.Images; // save the reference to all the images gathered from the document

                // now check the imported data against already existing issues
                ReferenceList = await _praccingService.GetPraccingIssues(SelectedSurvey.SID);
                LoadRecords(importer.ImportedIssues);


                Loaded = true;


            }
            catch (IOException)
            {
                _dialogService.ShowMessage("Unable to access the file. Make sure it is not open in the background and try again.");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message);
            }
            OnPropertyChanged(nameof(ItemPosition));
            
        }

        private void LoadRecords(List<PraccingIssue> issues)
        {
            List<StringPair> namesToFix = new List<StringPair>();   
            foreach (PraccingIssue issue in issues)
            {
                var matchingTo = PeopleList.FirstOrDefault(x => x.Name.Equals(issue.IssueTo.Name));
                if (matchingTo != null)
                    issue.IssueTo.ID = matchingTo.ID;
                else
                    namesToFix.Add(new StringPair(issue.IssueTo.Name));

                var matchingFrom = PeopleList.FirstOrDefault(x => x.Name.Equals(issue.IssueFrom.Name));
                if (matchingFrom != null)
                    issue.IssueFrom.ID = matchingFrom.ID;
                else
                    namesToFix.Add(new StringPair(issue.IssueFrom.Name));
            }

            //if (namesToFix.Count > 0)
            //{
            //    FixNames frm = new FixNames(namesToFix);
            //    frm.ShowDialog();
            //    if (frm.DialogResult == DialogResult.Cancel)
            //    {
            //        MessageBox.Show("Some names may appear blank.");
            //    }
            //}

            foreach (StringPair stringPair in namesToFix)
            {
                var matchingTo = issues.Where(x => x.IssueTo.Name.Equals(stringPair.String1));
                foreach (PraccingIssue issue in issues)
                {
                    issue.IssueTo.ID = PeopleList.FirstOrDefault(x => x.Name.Equals(stringPair.String2)).ID;
                }

                var matchingFrom = issues.Where(x => x.IssueFrom.Name.Equals(stringPair.String1));
                foreach (PraccingIssue issue in issues)
                {
                    issue.IssueFrom.ID = PeopleList.FirstOrDefault(x => x.Name.Equals(stringPair.String2)).ID;
                }
            }

            // now compare to existing issues
            foreach (PraccingIssue issue in issues)
            {
                var existingIssue = ReferenceList.FirstOrDefault(x => x.IssueNo == issue.IssueNo);
                if (existingIssue != null)
                {
                    issue.ID = existingIssue.ID;

                    foreach (PraccingResponse response in issue.Responses)
                    {
                        var matchingResponse = existingIssue.Responses.FirstOrDefault(x => Utilities.PrepareTextCompare(x.Response).Equals(Utilities.PrepareTextCompare(response.Response)));
                        if (matchingResponse != null)
                            response.ID = matchingResponse.ID;

                        response.IssueID = existingIssue.ID;
                    }
                }
            }
            
            foreach (PraccingIssue issue in issues)
            {
                if (issue.IssueNo <= 0 || issue.Responses.Any(x => x.ID <= 0))
                {
                    issue.EnteredBy = new Person(0); // TODO think about how to get user's name from username
                    ImportItems.Add(new PraccingImportItemViewModel(issue));
                }
            }

            CurrentItem = ImportItems.FirstOrDefault();
            
        }

        [RelayCommand]
        private async Task ImportIssues()
        {
            // gather the issues that are marked "keep"
            // for each issue, gather responses that are marked "keep"
            // save each issue in one API call
            // for issues that were saved successfully, remove them from the ImportItems list
            List<PraccingIssue> issuesToSave = new List<PraccingIssue>();
            var keepIssues = ImportItems.Where(x => x.KeepIssue).ToList();
            
            foreach(var issue in keepIssues)
            {
                PraccingIssue toSave = issue.Model.Copy();

                toSave.Responses = issue.NewResponses.Where(x => x.KeepResponse).Select(x => x.Response).ToList();
                
                issuesToSave.Add(toSave);

                if (toSave.ID > 0)
                {
                    var updated = await _praccingService.UpdatePraccingIssue(toSave); // update 
                    if (updated.ID > 0)
                    {
                        // move image file into folder
                        foreach (PraccingImage img in toSave.Images)
                        {
                            if (!File.Exists(DBImageRepo + @"\" + img.Path))
                                File.Copy(img.Path, DBImageRepo + @"\" + img.Path);
                            if (!img.Path.StartsWith(DBImageRepo))
                                img.Path = DBImageRepo + @"\" + img.Path;
                        }

                        foreach(PraccingImage img in toSave.Responses.SelectMany(x => x.Images))
                        {
                            if (!File.Exists(DBImageRepo + @"\" + img.Path))
                                File.Copy(img.Path, DBImageRepo + @"\" + img.Path);
                            if (!img.Path.StartsWith(DBImageRepo))
                                img.Path = DBImageRepo + @"\" + img.Path;
                        }


                        // update was successful, remove from import list
                        ImportItems.Remove(issue);
                    }
                }
                else
                {
                    var added = await _praccingService.AddPraccingIssue(toSave); // add 
                    if (added.ID > 0)
                    {
                        // move image file into folder
                        foreach (PraccingImage img in toSave.Images)
                        {
                            if (!File.Exists(DBImageRepo + @"\" + img.Path))
                                File.Copy(img.Path, DBImageRepo + @"\" + img.Path);
                            if (!img.Path.StartsWith(DBImageRepo))
                                img.Path = DBImageRepo + @"\" + img.Path;
                        }

                        foreach (PraccingImage img in toSave.Responses.SelectMany(x => x.Images))
                        {
                            if (!File.Exists(DBImageRepo + @"\" + img.Path))
                                File.Copy(img.Path, DBImageRepo + @"\" + img.Path);
                            if (!img.Path.StartsWith(DBImageRepo))
                                img.Path = DBImageRepo + @"\" + img.Path;
                        }

                        // add was successful, remove from import list
                        ImportItems.Remove(issue);
                    }
                }
            }
            OnPropertyChanged(nameof(ImportItems));
            CurrentItem = ImportItems.FirstOrDefault();
            //int issueCount = await SaveIssues();
            //int responseCount = await SaveResponses();

            //_dialogService.ShowMessage(issueCount + " issue(s) imported and " + responseCount + " response(s) imported.");
            _dialogService.ShowMessage("Done. Issues that were successfully saved have been removed from the import list.");
        }

        private async Task<int> SaveIssues()
        {
            int issueCount = 0;

            var importIssues = ImportItems.Where(x=>x.KeepIssue).Select(x => x.Model).ToList();

            // for each "saved" issue, add it to the database
            foreach (PraccingIssue pi in importIssues)
            {
                
                // update those that are already in the database
                if (pi.ID > 0)
                {
                    await _praccingService.UpdatePraccingIssue(pi);
                    continue;
                }

                int tempIssueNo = pi.IssueNo;
                if ((await _praccingService.AddPraccingIssue(pi)) == null)
                    pi.IssueNo = tempIssueNo;
                else
                {
                    foreach (PraccingResponse pr in pi.Responses)
                        pr.IssueID = pi.ID;

                    // save images
                    // copy images to network path
                    foreach (PraccingImage img in pi.Images)
                    {
                        if (!File.Exists(DBImageRepo + @"\" + img.Path))
                            File.Copy(img.Path, DBImageRepo + @"\" + img.Path);

                        if (!img.Path.StartsWith(DBImageRepo))
                            img.Path = DBImageRepo + @"\" + img.Path;
                    }


                  //  await _praccingService.AddPraccingImage(pi.ID, pi.Images);

                    issueCount++;
                }

            }
            // if the issue was inserted successfully, remove it from the list
            for (int i = 0; i < importIssues.Count; i++)
            {
                if (importIssues[i].ID > 0)
                    importIssues.Remove(importIssues[i]);
            }

            return issueCount;
        }

        private async Task<int> SaveResponses()
        {
            int responseCount = 0;
            var importResponses = ImportItems.Where(x=>x.HasNewResponses && x.NewResponses.Any(r=>r.KeepResponse))
                    .SelectMany(x => x.Model.Responses).ToList();
            // for each "saved" response, add it to the database
            foreach (PraccingResponse pr in importResponses)
            {
                // skip those that are already in the database
                if (pr.ID > 0)
                    continue;

                if (await _praccingService.AddPraccingResponse(pr) != null)
                    responseCount++;

                // save images
                // copy images to network path
                foreach (PraccingImage img in pr.Images)
                {
                    //if (File.Exists(AppImageRepo + @"\" + img.Path) && !File.Exists(DBImageRepo + @"\" + img.Path))
                    //    File.Copy(AppImageRepo + @"\" + img.Path, DBImageRepo + @"\" + img.Path);

                    if (!img.Path.StartsWith(DBImageRepo))
                        img.Path = DBImageRepo + @"\" + img.Path;
                }


                //await _praccingService.InsertPraccingResponseImage(pr.ID, pr.Images);

            }
            // if the response was inserted successfully, remove it from the list
            for (int i = 0; i < importResponses.Count; i++)
            {
                if (importResponses[i].ID > 0)
                    importResponses.Remove(importResponses[i]);
            }

            return responseCount;
        }

        

        #region Navigation Commands
        [RelayCommand]
        private void FirstIssue()
        {
            if (ImportItems == null)
                return;
            CurrentItem = ImportItems.First();
        }

        [RelayCommand]
        private void PreviousIssue()
        {
            if (ImportItems == null)
                return;
            
            int currentIndex = ImportItems.IndexOf(CurrentItem);
            if (currentIndex > 0)
            {
                CurrentItem = ImportItems[currentIndex - 1];
            }
            else
            {
                CurrentItem = ImportItems.Last();
            }
        }

        [RelayCommand]
        private void NextIssue()
        {
            
            if (ImportItems == null)
            {
                
                return;
            }
            int currentIndex = ImportItems.IndexOf(CurrentItem);
            if (currentIndex < ImportItems.Count - 1)
            {
                CurrentItem = ImportItems[currentIndex + 1];
            }
            else
            {
                CurrentItem = ImportItems.First();
            }
        }

        [RelayCommand]
        private void LastIssue()
        {
            if (CurrentItem == null || ImportItems == null || !ImportItems.Any())
                return;
            CurrentItem = ImportItems.Last();
        }

        #endregion

    }
}
