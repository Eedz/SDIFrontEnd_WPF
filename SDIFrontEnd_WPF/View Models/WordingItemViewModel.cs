using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITCLib;
using MvvmLib.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Windows.Markup;

namespace SDIFrontEnd_WPF.ViewModels
{
    /// <summary>
    /// ViewModel for a single Wording record. Owns editing, saving, and deleting one wording.
    /// </summary>
    public partial class WordingItemViewModel : ViewModelBase
    {
        private readonly IApiWordingService _wordingService;
        private readonly IDialogService _dialogService;

        public Wording Wording { get; }

        public bool IsNew => Wording.WordID == -1;

        [ObservableProperty]
        private bool lockedForEditing = true;

        [ObservableProperty]
        private ObservableCollection<WordingUsage> usages = new();

        [ObservableProperty]
        private FlowDocument wordingText = new();


        

        /// <summary>
        /// Raised when this item has been successfully deleted, so the parent list can remove it.
        /// </summary>
        public event EventHandler? DeleteRequested;

        /// <summary>
        /// Raised after a successful save of a new wording, so the parent can update its ID reference.
        /// </summary>
        public event EventHandler? SaveCompleted;

        public WordingItemViewModel(Wording wording, IApiWordingService wordingService, IDialogService dialogService)
        {
            Wording = wording ?? throw new ArgumentNullException(nameof(wording));
            _wordingService = wordingService ?? throw new ArgumentNullException(nameof(wordingService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            if (!IsNew)
            {
                LoadWordingText();

            }
            else
            {
                // New wordings start unlocked and ready to edit
                LockedForEditing = false;
            }
        }

        private void LoadWordingText()
        {
            if (string.IsNullOrEmpty(Wording.WordingText))
                return;

            WordingText = (FlowDocument)SimpleHtmlConverter.FromHtml(Wording.WordingText);
        }

        public async Task LoadUsagesAsync()
        {
            Usages = new ObservableCollection<WordingUsage>(
                await _wordingService.GetWordingUsages(Wording));
        }

        partial void OnWordingTextChanged(FlowDocument value)
        {
            Wording.WordingText = SimpleHtmlConverter.ToHtml(value);
        }

        [RelayCommand]
        private async Task ToggleEdit()
        {
            if (!LockedForEditing)
                await Save();

            LockedForEditing = !LockedForEditing;
        }

        [RelayCommand]
        private async Task Save()
        {
            if (Usages.Any(x => x.Locked))
            {
                _dialogService.ShowMessage("Cannot modify wordings used in locked surveys.");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(Wording.WordingText))
            {
                _dialogService.ShowError("Wording text cannot be empty.");
                return;
            }

            try
            {
                if (IsNew)
                    await _wordingService.CreateWording(Wording);
                else
                    await _wordingService.UpdateWording(Wording);

                await LoadUsagesAsync();
                SaveCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Error saving wording: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (Wording.WordID == 0)
            {
                _dialogService.ShowError("Cannot delete wording '0'.");
                return;
            }

            if (Usages.Count > 0)
            {
                _dialogService.ShowError("Cannot delete a wording that is in use.");
                return;
            }

            if (!_dialogService.Confirm($"Are you sure you want to delete {Wording.FieldType}#{Wording.WordID}? This action cannot be undone."))
                return;

            // Unsaved new wording — just signal removal, no API call needed
            if (IsNew)
            {
                DeleteRequested?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (await _wordingService.DeleteWording(Wording))
                DeleteRequested?.Invoke(this, EventArgs.Empty);
        }

    }
}
