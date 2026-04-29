using CommunityToolkit.Mvvm.Input;
using ITCLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmLib.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class VariableInformationViewModel : ViewModelBase
    {
        private readonly IApiVarNameService _service;
        private readonly IDialogService _dialogService;
        private readonly ReferenceDataStore _referenceDataStore;

        public ObservableCollection<VariableNameItemViewModel> Items { get; } = new();

        public ObservableCollection<VarNameLabel> Domains { get; }
        public ObservableCollection<VarNameLabel> Topics { get; }
        public ObservableCollection<VarNameLabel> Contents { get; }
        public ObservableCollection<VarNameLabel> Products { get; }

        [ObservableProperty]
        private string? filterString;

        [ObservableProperty]
        private bool exactMatch = false;

        public VariableInformationViewModel(
            IApiVarNameService service,
            IDialogService dialogService,
            ReferenceDataStore referenceDataStore)
        {
            _service = service;
            _dialogService = dialogService;
            _referenceDataStore = referenceDataStore;

            Domains = new ObservableCollection<VarNameLabel>(_referenceDataStore.DomainLabels);
            Topics = new ObservableCollection<VarNameLabel>(_referenceDataStore.TopicLabels);
            Contents = new ObservableCollection<VarNameLabel>(_referenceDataStore.ContentLabels);
            Products = new ObservableCollection<VarNameLabel>(_referenceDataStore.ProductLabels);          
        }

        partial void OnFilterStringChanged(string? oldValue, string? newValue)
        {
            _ = LoadFiltered(FilterString);
        }

        partial void OnExactMatchChanged(bool oldValue, bool newValue)
        {
            _ = LoadFiltered(FilterString);
        }

        private async Task LoadFiltered(string? filter)
        {
            if (filter == null) return;
            var data = await _service.SearchVarNames(filter, 200);

            if (ExactMatch)
            {
                data = data.Where(v => v.VarName.StartsWith(FilterString)).ToList();
            }

            Items.Clear();

            foreach (var item in data)
            {
                Items.Add(new VariableNameItemViewModel(
                    item,
                    _service,
                    _dialogService));
            }
            
        }

        [RelayCommand]
        private async Task SaveAll()
        {
            foreach (var item in Items)
            {
                if (item.Dirty) item.SaveCommand.Execute(null);
            }
        }
    }
}
