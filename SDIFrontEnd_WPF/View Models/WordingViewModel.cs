using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITC_Services;
using ITCLib;
using Microsoft.Data.SqlClient;
using MvvmLib.Converters;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Markup;

namespace SDIFrontEnd_WPF
{
    // TODO lock/unlock for editing (done)
    // TODO clipboard
    // TODO rich text editor for wording text (done)
    // TODO breaks missing in text
    // TODO reset state on move
    public partial class WordingViewModel : WorkspaceViewModel
    {
        private readonly IDialogService _dialogService; // Service for displaying dialogs to the user
        private readonly IWordingService _wordingService; // Service for managing question wordings and translations

        public string WordingType { get; set; } // type of wording being managed (e.g., PreP, PreI etc.)
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ItemPosition))]
        private Wording? currentWording; // currently selected wording 
        public ObservableCollection<Wording> Wordings { get; set; } = new ObservableCollection<Wording>();
        [ObservableProperty]
        private ObservableCollection<WordingUsage> usages = new ObservableCollection<WordingUsage>(); 

        [ObservableProperty]
        private bool lockedForEditing = true; // Flag indicating if the wording is locked for editing

        public string ItemPosition => $"{(Wordings.IndexOf(CurrentWording) + 1)} of {Wordings.Count}";

        private FlowDocument wordingText;
        public FlowDocument WordingText
        {
            get => wordingText;
            set
            {
                SetProperty(ref wordingText, value);
                CurrentWording.WordingText = HtmlUtils.ConvertFlowDocumentToHtml(value);
            }
        }
        public WordingViewModel(IWordingService wordingService, IDialogService dialogService, string type)
        {
            DisplayName = "Wording Manager";
            
            _wordingService = wordingService ?? throw new ArgumentNullException(nameof(wordingService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            WordingType = type ?? throw new ArgumentNullException(nameof(type));
            switch (type)
            {
                case "PreP": Wordings = new ObservableCollection<Wording>( _wordingService.GetAllPreP()); break;
                case "PreI": Wordings = new ObservableCollection<Wording>(_wordingService.GetAllPreI()); break;
                case "PreA": Wordings = new ObservableCollection<Wording>(_wordingService.GetAllPreA()); break;
                case "LitQ": Wordings = new ObservableCollection<Wording>(_wordingService.GetAllLitQ()); break;
                case "PstI": Wordings = new ObservableCollection<Wording>(_wordingService.GetAllPstI()); break;
                case "PstP": Wordings = new ObservableCollection<Wording>(_wordingService.GetAllPstP()); break;
            }

            CurrentWording = Wordings.FirstOrDefault();
        }

        public WordingViewModel(IWordingService wordingService, IDialogService dialogService, string type, int wordID) : this(wordingService, dialogService, type)
        {
            CurrentWording = Wordings.FirstOrDefault(w => w.WordID == wordID);
        }

        partial void OnCurrentWordingChanged(Wording? value)
        {
            if (value == null || value.WordID == 0)
            {
                Usages.Clear();
                return;
            }
            WordingText = (FlowDocument)XamlReader.Parse(HtmlToXaml.HtmlToXamlConverter.ConvertHtmlToXaml(value.WordingText, true));
            Usages = new ObservableCollection<WordingUsage>(_wordingService.GetWordingUsages(value));
            LockedForEditing = true;
        }

        [RelayCommand]
        private void EditWording()
        {
            if (!LockedForEditing)
            {
                SaveChanges();
            }
            LockedForEditing = !LockedForEditing;
        }

        [RelayCommand]
        private void CopyToNew(Wording wording)
        {
            Wording newWording = new Wording
            {
                WordID = -1,
                Type = wording.Type,
                WordingText = wording.WordingText
            };
            Wordings.Add(newWording);
            LockedForEditing = false;
            CurrentWording = newWording;
        }

        [RelayCommand]
        private void NewWording()
        {
            Wording newWording = new Wording
            {
                WordID = -1,
                Type = GetWordingType(),
                WordingText = string.Empty
            };
            Wordings.Add(newWording);
            LockedForEditing = false;
            CurrentWording = newWording;
        }

        [RelayCommand]
        private void DeleteWording(Wording wording)
        {
            if (wording.WordID == 0) // reserved
            {
                _dialogService.ShowError("Cannot delete wording '0'.");
                return;
            }

            if (Usages.Count > 0) // in use
            {
                _dialogService.ShowError("Cannot delete wording that is in use.");
                return;
            }

            if (_dialogService.Confirm($"Are you sure you want to delete {wording.FieldType}#{wording.WordID}? This action cannot be undone." ) == false)
            {
                return;
            }

            if (wording.WordID == -1) // new unsaved wording
            {
                Wordings.Remove(wording);
                CurrentWording = Wordings.LastOrDefault();
                return;
            }

            if (wording.WordID > 0)
            {
                PreviousItem();
                if (_wordingService.DeleteWording(wording.WordID, wording.FieldType) == 1)
                {
                    Wordings.Remove(wording);
                }
            }           
        }

        [RelayCommand]
        private void ClipSearch()
        {
            // TODO use clipboard service
            string criteria = _dialogService.PromptForText("Enter search text", "Search");
            var results = Wordings.Where(x => x.WordingText.Contains(criteria));
            Wordings = new ObservableCollection<Wording>(results);
            CurrentWording = Wordings.FirstOrDefault();
        }

        [RelayCommand]
        private void TextSearch()
        {
            string criteria = _dialogService.PromptForText("Enter search text", "Search");
            var results = Wordings.Where(x => x.WordingText.Contains(criteria));
            Wordings = new ObservableCollection<Wording>(results);
            CurrentWording = Wordings.FirstOrDefault();
        }

        [RelayCommand]
        private void SelectWording()
        {
            if (!LockedForEditing || (CurrentWording != null && CurrentWording.WordID == 0))
            {
                if (_dialogService.Confirm("You have unsaved changes. Save first?"))
                {
                    SaveChanges();
                    OnRequestClose(true);
                }
            }
            else
            {
                OnRequestClose(true);
            }
        }

        [RelayCommand]
        private void PreviousItem()
        {
            if (Wordings == null)
                return;
            if (CurrentWording == null)
            {
                CurrentWording = Wordings.FirstOrDefault();
                return;
            }
            int currentIndex = Wordings.IndexOf(CurrentWording);
            if (currentIndex > 0)
            {
                CurrentWording = Wordings[currentIndex - 1];
            }
            else
            {
                CurrentWording = Wordings.Last();
            }
        }

        [RelayCommand]
        private void NextItem()
        {
            if (Wordings == null)
                return;
            if (CurrentWording == null)
            {
                CurrentWording = Wordings.First();
                return;
            }
            int currentIndex = Wordings.IndexOf(CurrentWording);
            if (currentIndex < Wordings.Count - 1)
            {
                CurrentWording = Wordings[currentIndex + 1];
            }
            else
            {
                CurrentWording = Wordings.First();
            }
        }

        [RelayCommand]
        private void FirstItem()
        {
            if (Wordings == null)
                return;
           
            CurrentWording = Wordings.FirstOrDefault();
        }

        [RelayCommand]
        private void LastItem()
        {
            if (Wordings == null)
                return;
            
            CurrentWording = Wordings.LastOrDefault();
        }

        private ITCLib.WordingType GetWordingType()
        {
            switch(WordingType)
            {
                case "PreP": return ITCLib.WordingType.PreP;
                case "PreI": return ITCLib.WordingType.PreI;
                case "PreA": return ITCLib.WordingType.PreA;
                case "LitQ": return ITCLib.WordingType.LitQ;
                case "PstI": return ITCLib.WordingType.PstI;
                case "PstP": return ITCLib.WordingType.PstP;
                default: throw new ArgumentException("Invalid Wording Type");
            }
        }

        private void SaveChanges()
        {
            if (CurrentWording == null)
                return;
            if (string.IsNullOrWhiteSpace(CurrentWording.WordingText))
            {
                _dialogService.ShowError("Wording text cannot be empty.");
                return;
            }
            try
            {
                if (CurrentWording.WordID == -1)
                {
                    // New wording
                    int newId = _wordingService.InsertWording(CurrentWording);
                }
                else
                {
                    // Existing wording
                    _wordingService.UpdateWording(CurrentWording);
                }
                
                // Refresh usages after save
                Usages = new ObservableCollection<WordingUsage>(_wordingService.GetWordingUsages(CurrentWording));
            }
            catch(SqlException sqle)
            {

            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error saving wording: {ex.Message}");
            }
        }
    }
}
