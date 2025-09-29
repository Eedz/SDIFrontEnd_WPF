using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmLib.ViewModels;
using ITC_Services;
using ITCLib;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SDIFrontEnd_WPF
{
    public partial class WordingViewModel : WorkspaceViewModel
    {
        private readonly IDialogService _dialogService; // Service for displaying dialogs to the user
        private readonly IWordingService _wordingService; // Service for managing question wordings and translations

        public string WordingType { get; set; } // Type of wording being managed (e.g., PreP, PreI etc.)
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ItemPosition))]
        private Wording? currentWording; // The currently selected wording 
        public ObservableCollection<Wording> Wordings { get; set; } = new ObservableCollection<Wording>(); // Collection of wordings for the UI
        [ObservableProperty]
        private ObservableCollection<WordingUsage> usages = new ObservableCollection<WordingUsage>(); // Collection of wordings for the UI

        [ObservableProperty]
        private bool lockedForEditing = true; // Flag indicating if the wording is locked for editing

        public string ItemPosition => $"{(Wordings.IndexOf(CurrentWording) + 1)} of {Wordings.Count}";

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
            if (value.WordID == 0)
            {
                Usages.Clear();
                return;
            }

            Usages = new ObservableCollection<WordingUsage>(_wordingService.GetWordingUsages(CurrentWording));
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
            CurrentWording = newWording;
        }

        [RelayCommand]
        private void DeleteWording(Wording wording)
        {
            if (wording.WordID == 0)
            {
                _dialogService.ShowError("Cannot delete wording '0'.");
                return;
            }

            if (wording.WordID > 0)
            {
                _wordingService.DeleteWording(wording.WordID, wording.FieldType);
            }           
               
            Wordings.Remove(wording);
            CurrentWording = Wordings.FirstOrDefault();
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
                LockedForEditing = true;
                // Refresh usages after save
                Usages = new ObservableCollection<WordingUsage>(_wordingService.GetWordingUsages(CurrentWording));
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error saving wording: {ex.Message}");
            }
        }
    }
}
