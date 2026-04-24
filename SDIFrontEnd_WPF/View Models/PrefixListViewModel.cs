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
        private readonly IDialogService _dialogService;
        private readonly IApiQuestionService _questionService;

        [ObservableProperty]
        private VariablePrefixItemViewModel? selectedRecord;

        [ObservableProperty]
        private ObservableCollection<VariableNameSurveys> usages = new();

        public ObservableCollection<VariableNameSurveys> FilteredUsages
        {
            get
            {
                if (FilterOnRange && SelectedRange != null)
                {
                    return new ObservableCollection<VariableNameSurveys>(
                        Usages.Where(x => x.NumberInt() >= SelectedRange.LowerInt() &&
                        x.NumberInt() <= SelectedRange.UpperInt()));
                }
                else
                {
                    return Usages;
                }
            }
        }

        [ObservableProperty]
        private VariableRange? selectedRange;

        [ObservableProperty]
        private bool filterOnRange;

        [ObservableProperty]
        private VariableNameSurveys? selectedUsage;

        

        public PrefixListViewModel(IApiVarNameService varnameService, IDialogService dialogService, IApiQuestionService questionService)
        {
            _service = varnameService;
            _dialogService = dialogService;
            _questionService = questionService;
        }        

        partial void OnSelectedRecordChanged(VariablePrefixItemViewModel? value)
        {
            if (value == null) return;

            _ = LoadUsagesAsync();
            OnPropertyChanged(nameof(FilteredUsages));
        }

        partial void OnFilterOnRangeChanged(bool oldValue, bool newValue)
        {
            OnPropertyChanged(nameof(FilteredUsages));
        }

        partial void OnSelectedRangeChanged(VariableRange? oldValue, VariableRange? newValue)
        {
            OnPropertyChanged(nameof(FilteredUsages));
        }

        public async Task Load()
        {
            var prefixes = await _service.GetVariablePrefixes();

            foreach (var prefix in prefixes)
            {
                Records.Add(new VariablePrefixItemViewModel(prefix));
            }
        }

        private async Task LoadUsagesAsync()
        {
            if (SelectedRecord == null) return;

            var result = await _service.SearchVarNameUsage(SelectedRecord.Prefix, 1000);

            Usages = new ObservableCollection<VariableNameSurveys>(result);
            OnPropertyChanged(nameof(FilteredUsages));  
        }
            
        [RelayCommand]
        private void AddPrefix()
        {
            var record = new VariablePrefixItemViewModel(new VariablePrefix());
            record.IsNew = true;

            Records.Add(record);
            SelectedRecord = record;
        }

        [RelayCommand]
        private async Task DeletePrefix()
        {
            if (SelectedRecord == null) return;

            if (_dialogService.Confirm("Are you sure you want to delete this prefix? (This will not delete any variable names with this prefix)"))
            {
                if (await _service.DeleteVariablePrefix(SelectedRecord.ID))
                    Records.Remove(SelectedRecord);
                else
                    _dialogService.ShowMessage("Failed to delete the prefix.");

            }
        }

        [RelayCommand]
        private void Save()
        {
            if (SelectedRecord == null) return;
            SelectedRecord.Model.Ranges = SelectedRecord.Ranges.ToList();
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

        [RelayCommand]
        private async Task ShowWordings()
        {
            if (SelectedRecord == null) return;

            var questions = await _service.GetVarNameQuestions(SelectedUsage.VarName);

            

            var vm = new VarNameUsageViewModel(questions);
            

            _dialogService.ShowDialog(vm);
        }

        [RelayCommand]
        private async Task Print()
        {
            
        }
    }
}