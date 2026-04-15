using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITCLib;
using MvvmLib.ViewModels;
using SDIFrontEnd_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    public partial class ResponseSetViewModel : WorkspaceViewModel
    {
        private readonly IDialogService _dialogService; // Service for displaying dialogs to the user
        private readonly IApiWordingService _wordingService; // Service for managing question wordings and translations
        private readonly WordingData _wordingData;

        public string ResponseType { get; set; } // Type of wording being managed (e.g., PreP, PreI etc.)

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ItemPosition))]
        private ResponseSetItemViewModel? currentItem;
        public ObservableCollection<ResponseSetItemViewModel> Items { get; set; } = new();

        public string ItemPosition
        {
            get
            {
                if (CurrentItem == null) return $"0 of {_allWordings.Count}";
                int index = _allWordings.IndexOf(CurrentItem.ResponseSet);
                return $"{(index + 1)} of {_allWordings.Count}";
            }
        }

        private List<ResponseSet> _allWordings = new();
        private string? _initialSet= null;       

        public ResponseSetViewModel(WordingData wordingData, IApiWordingService wordingService, IDialogService dialogService, string type)
        {
            DisplayName = "Response Set Manager";

            _wordingService = wordingService ?? throw new ArgumentNullException(nameof(wordingService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _wordingData = wordingData;

            ResponseType = type ?? throw new ArgumentNullException(nameof(type));           
        }

        public ResponseSetViewModel(WordingData wordingData, IApiWordingService wordingService, IDialogService dialogService, string type, string respset) : 
            this(wordingData, wordingService, dialogService, type)
        {
            _initialSet = respset;
        }

        public async Task Load()
        {
            _allWordings = ResponseType switch
            {
                "RespOptions" => _wordingData.RO.ToList(),
                "NRCodes" => _wordingData.NR.ToList(),
                _ => new List<ResponseSet>()
            };

            var initial = _initialSet == null
                ? _allWordings.FirstOrDefault()
                : _allWordings.FirstOrDefault(w => w.RespSetName == _initialSet) ?? _allWordings.FirstOrDefault();

            NavigateTo(initial);
        }

        /// <summary>
        /// Create a ViewModel for the specified wording and set the CurrentItem.
        /// </summary>
        /// <param name="wording"></param>
        private void NavigateTo(ResponseSet? wording)
        {
            if (wording == null) return;

            if (CurrentItem != null)
                CurrentItem.DeleteRequested -= OnItemDeleteRequested;

            var item = new ResponseSetItemViewModel(wording, _wordingService, _dialogService);
            item.DeleteRequested += OnItemDeleteRequested;
            _ = item.LoadUsagesAsync();
            CurrentItem = item;
        }

        private void OnItemDeleteRequested(object? sender, EventArgs e)
        {
            if (sender is not ResponseSetItemViewModel item)
                return;

            item.DeleteRequested -= OnItemDeleteRequested;

            int index = _allWordings.IndexOf(item.ResponseSet);
            _allWordings.Remove(item.ResponseSet);

            NavigateTo(_allWordings.Count > 0
                ? _allWordings[Math.Max(0, index - 1)]
                : null);
        }

        partial void OnCurrentItemChanged(ResponseSetItemViewModel? value)
        {
            if (value != null)
                _ = value.LoadUsagesAsync();
        }

        [RelayCommand]
        private void CopyToNew(ResponseSet respSet)
        {
            ResponseSet newWording = new ResponseSet
            {
                RespSetName = string.Empty,
                Type = respSet.Type,
                RespList = respSet.RespList
            };
            _allWordings.Add(newWording);
            NavigateTo(newWording);
        }

        [RelayCommand]
        private void NewWording()
        {
            ResponseSet newWording = new ResponseSet
            {
                RespSetName = string.Empty,
                Type = GetResponseType(),
                RespList = string.Empty
            };
            _allWordings.Add(newWording);
            NavigateTo(newWording);
        }
     

        [RelayCommand]
        private void ClipSearch()
        {
            // TODO use clipboard service
            //string criteria = _dialogService.PromptForText("Enter search text", "Search");
            //var results = _allWordings.Where(x => x.RespList.Contains(criteria));
            //_allWordings = new ObservableCollection<ResponseSet>(results);
            
        }

        [RelayCommand]
        private void TextSearch()
        {
            string criteria = _dialogService.PromptForText("Enter search text", "Search");
            if (string.IsNullOrWhiteSpace(criteria))
                return;

            _allWordings = _allWordings.Where(w => w.RespList.Contains(criteria)).ToList();
            OnPropertyChanged(nameof(ItemPosition));
            NavigateTo(_allWordings.FirstOrDefault());
        }

        [RelayCommand]
        private void SelectWording()
        {
            bool hasUnsavedChanges = CurrentItem != null && !CurrentItem.LockedForEditing && string.IsNullOrEmpty(CurrentItem.ResponseSet.RespSetName);

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

            int i = _allWordings.IndexOf(CurrentItem.ResponseSet);
            NavigateTo(_allWordings[i > 0 ? i - 1 : _allWordings.Count - 1]);
        }

        [RelayCommand]
        private void NextItem()
        {
            if (_allWordings.Count == 0) return;
            if (CurrentItem == null) { NavigateTo(_allWordings.First()); return; }

            int i = _allWordings.IndexOf(CurrentItem.ResponseSet);
            NavigateTo(_allWordings[(i + 1) % _allWordings.Count]);
        }

        [RelayCommand]
        private void FirstItem() => NavigateTo(_allWordings.FirstOrDefault());

        [RelayCommand]
        private void LastItem() => NavigateTo(_allWordings.LastOrDefault());

        private ITCLib.ResponseType GetResponseType()
        {
            switch (ResponseType)
            {
                case "RespOptions": return ITCLib.ResponseType.RespOptions;
                case "NRCodes": return ITCLib.ResponseType.NRCodes;
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
