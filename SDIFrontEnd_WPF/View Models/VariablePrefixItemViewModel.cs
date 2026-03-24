using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.Wordprocessing;
using ITCLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmLib.ViewModels;

namespace SDIFrontEnd_WPF.View_Models
{
    public partial class VariablePrefixItemViewModel : ViewModelBase
    {

        

        public VariablePrefix Model { get; }
        private VariablePrefix _original;

        [ObservableProperty]
        private bool isDirty;

        [ObservableProperty]
        private bool isNew;

        public ObservableCollection<VariableRange> Ranges { get; }

        public VariablePrefixItemViewModel(VariablePrefix model)
        {
            Model = model;
            Ranges = new ObservableCollection<VariableRange>(model.Ranges);
            
            _original = new VariablePrefix()
            {
                ID = model.ID,
                Prefix = model.Prefix,
                PrefixName = model.PrefixName,
                ProductType = model.ProductType,
                RelatedPrefixes = model.RelatedPrefixes,
                Description = model.Description,
                Comments = model.Comments,
                Inactive = model.Inactive
            };
        }

        public int ID { get => Model.ID; }

        public string Prefix
        {
            get => Model.Prefix;
            set
            {
                if (Model.Prefix != value)
                {
                    Model.Prefix = value;
                    IsDirty = true;
                    OnPropertyChanged();
                }
            }
        }

        public string PrefixName
        {
            get => Model.PrefixName;
            set
            {
                if (Model.PrefixName != value)
                {
                    Model.PrefixName = value;
                    IsDirty = true;
                    OnPropertyChanged();
                }
            }
        }

        public string ProductType
        {
            get => Model.ProductType;
            set
            {
                if (Model.ProductType != value)
                {
                    Model.ProductType = value;
                    IsDirty = true;
                    OnPropertyChanged();
                }
            }
        }

        
    }
}
