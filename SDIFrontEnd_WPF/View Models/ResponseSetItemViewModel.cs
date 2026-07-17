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
    /// ViewModel for a single Response record. Owns editing, saving, and deleting one response.
    /// </summary>
    public partial class ResponseSetItemViewModel : ViewModelBase
    {
        private readonly IApiWordingService _wordingService;
        private readonly IDialogService _dialogService;

        public ResponseSet ResponseSet { get; }

        public bool IsNew => ResponseSet.RespSetName == string.Empty;
        [ObservableProperty]
        private bool lockedForEditing = true;

        [ObservableProperty]
        private ObservableCollection<ResponseUsage> usages = new();

        [ObservableProperty]
        private string wordingHtml = string.Empty;


        /// <summary>
        /// Raised when this item has been successfully deleted, so the parent list can remove it.
        /// </summary>
        public event EventHandler? DeleteRequested;

        /// <summary>
        /// Raised after a successful save of a new wording, so the parent can update its ID reference.
        /// </summary>
        public event EventHandler? SaveCompleted;

        public ResponseSetItemViewModel(ResponseSet responseSet, IApiWordingService wordingService, IDialogService dialogService)
        {
            ResponseSet = responseSet ?? throw new ArgumentNullException(nameof(responseSet));
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
            if (string.IsNullOrEmpty(ResponseSet.RespList))
                return;

            WordingHtml = ResponseSet.RespList;
        }

        public async Task LoadUsagesAsync()
        {
            Usages = new ObservableCollection<ResponseUsage>(
                await _wordingService.GetResponseUsages(ResponseSet));
        }

        partial void OnWordingHtmlChanged(string value)
        {
            ResponseSet.RespList = value == null
                ? string.Empty
                : value;
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

            if (string.IsNullOrWhiteSpace(ResponseSet.RespList))
            {
                _dialogService.ShowError("Response text cannot be empty.");
                return;
            }
            
            try
            {
                if (IsNew)
                    await _wordingService.CreateResponseSet(ResponseSet);
                else
                    await _wordingService.UpdateResponseSet(ResponseSet);

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
            if (ResponseSet.RespSetName == "0")
            {
                _dialogService.ShowError("Cannot delete response set '0'.");
                return;
            }

            if (Usages.Count > 0)
            {
                _dialogService.ShowError("Cannot delete a response set that is in use.");
                return;
            }

            if (!_dialogService.Confirm($"Are you sure you want to delete {ResponseSet.Type}#{ResponseSet.RespSetName}? This action cannot be undone."))
                return;

            // Unsaved new response set
            if (IsNew)
            {
                DeleteRequested?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (await _wordingService.DeleteResponseSet(ResponseSet))
                DeleteRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
