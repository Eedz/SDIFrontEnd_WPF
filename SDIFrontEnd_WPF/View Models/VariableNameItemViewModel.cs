using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmLib.ViewModels;
namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class VariableNameItemViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IApiVarNameService _service;

        public VariableName Model { get; }

        private VariableName _original;

        public VariableNameItemViewModel(VariableName model, IApiVarNameService service, IDialogService dialogService)
        {
            Model = model;
            _service = service;
            _dialogService = dialogService;

            _original = new VariableName(model);
        }

        public string VarName => Model.VarName;
        public string RefVarName => Model.RefVarName;

        [ObservableProperty]
        private bool dirty = false;

        public string VarLabel
        {
            get => Model.VarLabel;
            set
            {
                SetProperty(Model.VarLabel, value, Model, (m, v) => m.VarLabel = v);
                Dirty = true;
            }
        }

        public VarNameLabel Domain
        {
            get => Model.DomainLabel;
            set {
                SetProperty(Model.DomainLabel, value, Model, (m, v) => m.DomainLabel = v);
                Dirty = true;
            }
        }

        public VarNameLabel Topic
        {
            get => Model.TopicLabel;
            set
            {
                SetProperty(Model.TopicLabel, value, Model, (m, v) => m.TopicLabel = v);
                Dirty = true;
            }
        }

        public VarNameLabel Content
        {
            get => Model.ContentLabel;
            set
            {
                SetProperty(Model.ContentLabel, value, Model, (m, v) => m.ContentLabel = v);
                Dirty = true;
            }
        }

        public VarNameLabel Product
        {
            get => Model.ProductLabel;
            set
            {
                SetProperty(Model.ProductLabel, value, Model, (m, v) => m.ProductLabel = v);
                Dirty = true;
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (!_dialogService.Confirm("Save changes?"))
            {
                Cancel();
                return;
            }

            bool success = await _service.UpdateVariable(Model);
            if (success)
            {
                Dirty = false;
                _original = new VariableName(Model);
            }
             

        }

        [RelayCommand]
        private void Cancel()
        {
            Model.VarLabel = _original.VarLabel;
            Model.Domain = _original.Domain;
            Model.Topic = _original.Topic;
            Model.Content = _original.Content;
            Model.Product = _original.Product;

            OnPropertyChanged(string.Empty); // refresh all bindings
        }

        [RelayCommand]
        private async Task Delete()
        {
            bool inUse = await _service.VarNameInUse(Model.VarName);

            if (inUse)
            {
                _dialogService.ShowMessage(
                    "This variable is used in one or more surveys and cannot be deleted.");
                return;
            }

            if (!_dialogService.Confirm("Are you sure you want to delete this variable?"))
                return;

            await _service.DeleteVariable(Model.VarName);
        }
    }
}
