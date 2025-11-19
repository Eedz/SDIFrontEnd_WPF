using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmLib.ViewModels;
using ITC_Services;
using ITCLib;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial  class TranslationImporterViewModel : WorkspaceViewModel
    {

        private readonly ISurveyService _surveyService;
        private readonly IReferenceDataService _referenceDataService;
        private readonly IDialogService _fileDialogService;

        public List<Translation> Missing { get; set; } = new List<Translation>();
        public List<Translation> Duplicate { get; set; } = new List<Translation>();
        public List<Translation> Imported { get; set; } = new List<Translation>();
         
        [ObservableProperty]
        private int missingIndex;
        [ObservableProperty]
        private int duplicateIndex;
        [ObservableProperty]
        private int importedIndex;

        public Translation MissingCurrent => Missing.Count > 0 && MissingIndex >= 0 ? Missing[MissingIndex] : null;
        public Translation DuplicateCurrent => Duplicate.Count > 0 && DuplicateIndex >= 0 ? Duplicate[DuplicateIndex] : null;
        public Translation ImportedCurrent => Imported.Count > 0 && ImportedIndex >= 0 ? Imported[ImportedIndex] : null;

        public string CurrentImportedText => ImportedCurrent != null ? ImportedCurrent.TranslationText : string.Empty;
        public string CurrentMissingText => MissingCurrent != null ? MissingCurrent.TranslationText : string.Empty;
        public string CurrentDuplicateText => DuplicateCurrent != null ? DuplicateCurrent.TranslationText : string.Empty;

        public List<Survey> SurveyList { get; }
        public List<Language> LanguageList { get; set; }

        [ObservableProperty]
        private string sourceFile;

        public Survey SelectedSurvey { get; set; }
        public Language SelectedLanguage { get; set; }

        public TranslationImporterViewModel(ISurveyService surveyService, IReferenceDataService referenceDataService, IDialogService fileDialogService)
        {
            _surveyService = surveyService;
            _referenceDataService = referenceDataService;
            _fileDialogService = fileDialogService;

            SurveyList = _surveyService.GetAllSurveys();
            LanguageList = _referenceDataService.GetLanguages();

            this.DisplayName = "Translation Importer";

            MissingIndex = 0;
            DuplicateIndex = 0;
            ImportedIndex = 0;
            
        }

        /// <summary>
        /// Prompt the user to specify the source file
        /// </summary>
        [RelayCommand]
        private void BrowseForFile()
        {
            string filepath = _fileDialogService.OpenFile("Word Documents|*.docx");

            if (!string.IsNullOrEmpty(filepath))
            {
                SourceFile = filepath;
            }
        }

        /// <summary>
        /// Read the source file and create Translation objects
        /// </summary>
        /// <param name="filepath"></param>
        [RelayCommand]
        private void Import(string filepath)
        {
            var tableImporter = new WordTableImporter(SourceFile);
            var parser = new TranslationParser();

            var importer = new WordImporter<Translation>(tableImporter, parser);

            List<Translation> translations = importer.Import().ToList();

            // get reference survey questions
            List<SurveyQuestion> questions = _surveyService.GetQuestionsForSurvey(SelectedSurvey.SID);
            foreach (Translation t in translations)
            {
                t.Survey = SelectedSurvey.SurveyCode;
                t.LanguageName = SelectedLanguage;

                SurveyQuestion match = questions.FirstOrDefault(q => q.VarName.VarName.Equals(t.VarName, StringComparison.OrdinalIgnoreCase));
                if (match == null)
                {
                    Missing.Add(t);
                    continue;
                }

                Translation existingTranslation = match.Translations.FirstOrDefault(x => x.Language.Equals(t.Language));    

                t.ID = existingTranslation != null ? existingTranslation.ID : 0; // get from DB else 0
                t.QID = match.ID;

                if (Imported.Contains(t))
                    Duplicate.Add(t);
                else
                    Imported.Add(t);
            }

            
            OnPropertyChanged(nameof(Imported));
            OnPropertyChanged(nameof(Missing));
            OnPropertyChanged(nameof(Duplicate));

            OnPropertyChanged(nameof(CurrentImportedText));
            OnPropertyChanged(nameof(CurrentMissingText));
            OnPropertyChanged(nameof(CurrentDuplicateText));
        }

        /// <summary>
        /// Commit all the Translations in the Imported list to the database.
        /// </summary>
        [RelayCommand]
        private void Save()
        {
            foreach(Translation t in Imported)
            {
                //_surveyService.UpdateTranslation(t);
            }
        }

        /// <summary>
        /// Checks if the given variable name is part of the survey.
        /// </summary>
        [RelayCommand]
        
        private void CheckVarName(Translation translation)
        {
            int qid = _surveyService.GetQuestionID(translation.Survey, translation.VarName);
            if (qid != 0)
            {
                translation.QID = qid;
                Missing.Remove(translation);
                Imported.Add(translation);
                OnPropertyChanged(nameof(Missing));
                OnPropertyChanged(nameof(Imported));
            }
        }

        // Nav Commands
        [RelayCommand]
        private void MissingNext()
        {
            if (MissingIndex < Missing.Count - 1)
            {
                MissingIndex++;
                OnPropertyChanged(nameof(MissingCurrent));
                OnPropertyChanged(nameof(CurrentMissingText));
            }
        }

        [RelayCommand]
        private void MissingPrev()
        {
            if (MissingIndex > 0)
            {
                MissingIndex--;
                OnPropertyChanged(nameof(MissingCurrent));
                OnPropertyChanged(nameof(CurrentMissingText));
            }
        }

        [RelayCommand]
        private void DuplicateNext()
        {
            if (DuplicateIndex < Duplicate.Count - 1)
            {
                DuplicateIndex++;
                OnPropertyChanged(nameof(DuplicateCurrent));
                OnPropertyChanged(nameof(CurrentDuplicateText));
            }
        }

        [RelayCommand]
        private void DuplicatePrev()
        {
            if (DuplicateIndex > 0)
            {
                DuplicateIndex--;
                OnPropertyChanged(nameof(DuplicateCurrent));
                OnPropertyChanged(nameof(CurrentDuplicateText));
            }
        }

        [RelayCommand]
        private void ImportedNext()
        {
            if (ImportedIndex < Imported.Count - 1)
            {
                ImportedIndex++;
                OnPropertyChanged(nameof(ImportedCurrent));
                OnPropertyChanged(nameof(CurrentImportedText));
            }
        }

        [RelayCommand]
        private void ImportedPrev()
        {
            if (ImportedIndex > 0)
            {
                ImportedIndex--;
                OnPropertyChanged(nameof(ImportedCurrent));
                OnPropertyChanged(nameof(CurrentImportedText));
            }
        }
    }
}
