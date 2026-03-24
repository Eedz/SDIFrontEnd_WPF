using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ITCLib;
using System.Collections.ObjectModel;
using MvvmLib.ViewModels;
using SDIFrontEnd_WPF.View_Models;

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class PrefixListViewModel : ViewModelBase
    {
        public ObservableCollection<VariablePrefixItemViewModel> Records { get; } = new();

        private readonly IApiVarNameService _service;

        [ObservableProperty]
        private VariablePrefixItemViewModel? selectedRecord;

        [ObservableProperty]
        private ObservableCollection<VariableNameSurveys> usages = new();

        [ObservableProperty]
        private VariableRange? selectedRange;

        public PrefixListViewModel(IApiVarNameService varnameService)
        {
            _service = varnameService;

            
        }

        private async Task Load()
        {
            var prefixes = await  _service.GetVariablePrefixes();

            foreach (var prefix in prefixes)
            {
                Records.Add(new VariablePrefixItemViewModel(prefix));
            }
        }

        partial void OnSelectedRecordChanged(VariablePrefixItemViewModel? value)
        {
            if (value == null) return;

            LoadUsagesAsync();
        }

        private async void LoadUsagesAsync()
        {
            if (SelectedRecord == null) return;

            var result = await _service.SearchVarNameUsage(SelectedRecord.Prefix,1000);

            Usages = new ObservableCollection<VariableNameSurveys>(result);
        }

        // -----------------------
        // Commands
        // -----------------------

        [RelayCommand]
        private void AddPrefix()
        {
            var record = new VariablePrefixItemViewModel(new VariablePrefix());
            record.IsNew = true;

            Records.Add(record);
            SelectedRecord = record;
        }

        [RelayCommand]
        private void DeletePrefix()
        {
            if (SelectedRecord == null) return;

            _service.DeleteVariablePrefix(SelectedRecord.ID);
            Records.Remove(SelectedRecord);
        }

        [RelayCommand]
        private void Save()
        {
            if (SelectedRecord == null) return;
            if (SelectedRecord.IsNew)
                _service.InsertVariablePrefix(SelectedRecord.Model);
            else if (SelectedRecord.IsDirty)
                _service.UpdateVariablePrefix(SelectedRecord.Model);
        }

        [RelayCommand]
        private void AddRange()
        {
            if (SelectedRecord == null) return;

            var range = new VariableRange
            {
                PrefixID = SelectedRecord.ID
            };

            SelectedRecord.Ranges.Add(range);
        }

        [RelayCommand]
        private void DeleteRange()
        {
            if (SelectedRecord == null || SelectedRange == null) return;

            SelectedRecord.Ranges.Remove(SelectedRange);
        }
    }
}