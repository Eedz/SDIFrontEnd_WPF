using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITCLib;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SDIFrontEnd_WPF.ViewModels
{
    // TODO clipboard
    // TODO breaks missing in text
    public partial class WordingViewModel : WorkspaceViewModel
    {
        private readonly IDialogService _dialogService; // Service for displaying dialogs to the user
        private readonly IApiWordingService _wordingService; // Service for managing question wordings and translations
        private readonly WordingData _wordingData;

        public string WordingType { get; set; } // type of wording being managed (e.g., PreP, PreI etc.)

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ItemPosition))]
        private WordingItemViewModel? currentItem;
        public ObservableCollection<WordingItemViewModel> Items { get; set; } = new();

        public string ItemPosition
        {
            get
            {
                if (CurrentItem == null) return $"0 of {_allWordings.Count}";
                int index = _allWordings.IndexOf(CurrentItem.Wording);
                return $"{(index + 1)} of {_allWordings.Count}";
            }
        }

        private List<Wording> _allWordings = new();
        private int? _initialWordID = null;

        public WordingViewModel(WordingData wordingData, IApiWordingService wordingService, IDialogService dialogService, string type)
        {
            DisplayName = "Wording Manager";
            
            _wordingService = wordingService ?? throw new ArgumentNullException(nameof(wordingService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _wordingData = wordingData ?? throw new ArgumentNullException(nameof(wordingData));
            WordingType = type ?? throw new ArgumentNullException(nameof(type));
        }

        public WordingViewModel(WordingData wordingData, IApiWordingService wordingService, IDialogService dialogService, string type, int wordID) 
            : this(wordingData, wordingService, dialogService, type)
        {
            _initialWordID = wordID;
        }

        public async Task Load()
        {
            _allWordings = WordingType switch
            {
                "PreP" => _wordingData.PreP.ToList(),
                "PreI" => _wordingData.PreI.ToList(),
                "PreA" => _wordingData.PreA.ToList(),
                "LitQ" => _wordingData.LitQ.ToList(),
                "PstI" => _wordingData.PstI.ToList(),
                "PstP" => _wordingData.PstP.ToList(),
                _ => throw new ArgumentException($"Unknown wording type: {WordingType}")
            };

            var initial = _initialWordID.HasValue
                ? _allWordings.FirstOrDefault(w => w.WordID == _initialWordID.Value) ?? _allWordings.FirstOrDefault()
                : _allWordings.FirstOrDefault();

            NavigateTo(initial);
        }

        /// <summary>
        /// Create a ViewModel for the specified wording and set the CurrentItem.
        /// </summary>
        /// <param name="wording"></param>
        private void NavigateTo(Wording? wording)
        {
            if (wording == null) return;

            if (CurrentItem != null)
                CurrentItem.DeleteRequested -= OnItemDeleteRequested;

            var item = new WordingItemViewModel(wording, _wordingService, _dialogService);
            item.DeleteRequested += OnItemDeleteRequested;
            _ = item.LoadUsagesAsync();
            CurrentItem = item;
        }

        private void OnItemDeleteRequested(object? sender, EventArgs e)
        {
            if (sender is not WordingItemViewModel item)
                return;

            item.DeleteRequested -= OnItemDeleteRequested;

            int index = _allWordings.IndexOf(item.Wording);
            _allWordings.Remove(item.Wording);

            NavigateTo(_allWordings.Count > 0
                ? _allWordings[Math.Max(0, index - 1)]
                : null);
        }

        partial void OnCurrentItemChanged(WordingItemViewModel? value)
        {
            if (value != null)  
                _ = value.LoadUsagesAsync();
        }

        [RelayCommand]
        private void CopyToNew(WordingItemViewModel source)
        {
            var newWording = new Wording
            {
                WordID = -1,
                Type = source.Wording.Type,
                WordingText = source.Wording.WordingText
            };
            _allWordings.Add(newWording);
            NavigateTo(newWording);
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

            _allWordings.Add(newWording);
            NavigateTo(newWording);

            //Wordings.Add(newWording);
            //LockedForEditing = false;
            //CurrentWording = newWording;
        }

        [RelayCommand]
        private void ClipSearch()
        {
            // TODO use clipboard service
            //string criteria = _dialogService.PromptForText("Enter search text", "Search");
            //var results = Wordings.Where(x => x.WordingText.Contains(criteria));
            //Wordings = new ObservableCollection<Wording>(results);
            //CurrentWording = Wordings.FirstOrDefault();
        }

        [RelayCommand]
        private void TextSearch()
        {
            string criteria = _dialogService.PromptForText("Enter search text", "Search");
            if (string.IsNullOrWhiteSpace(criteria))
                return;

            _allWordings = _allWordings.Where(w => w.WordingText.Contains(criteria)).ToList();
            OnPropertyChanged(nameof(ItemPosition));
            NavigateTo(_allWordings.FirstOrDefault());
        }

        [RelayCommand]
        private void SelectWording()
        {
            bool hasUnsavedChanges = CurrentItem != null && !CurrentItem.LockedForEditing && CurrentItem.Wording.WordID == -1;

            if (hasUnsavedChanges)
            {
                if (_dialogService.Confirm("You have unsaved changes. Save first?"))
                {
                    CurrentItem!.SaveCommand.Execute(null);
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
            if (_allWordings.Count == 0) return;
            if (CurrentItem == null) { NavigateTo(_allWordings.First()); return; }

            int i = _allWordings.IndexOf(CurrentItem.Wording);
            NavigateTo(_allWordings[i > 0 ? i - 1 : _allWordings.Count - 1]);
        }

        [RelayCommand]
        private void NextItem()
        {
            if (_allWordings.Count == 0) return;
            if (CurrentItem == null) { NavigateTo(_allWordings.First()); return; }

            int i = _allWordings.IndexOf(CurrentItem.Wording);
            NavigateTo(_allWordings[(i + 1) % _allWordings.Count]);
        }

        [RelayCommand]
        private void FirstItem() => NavigateTo(_allWordings.FirstOrDefault());

        [RelayCommand]
        private void LastItem() => NavigateTo(_allWordings.LastOrDefault());

        private WordingType GetWordingType()
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

        protected override void OnDispose()
        {
            if (CurrentItem != null)
                CurrentItem.DeleteRequested -= OnItemDeleteRequested;
        }
    }
}
