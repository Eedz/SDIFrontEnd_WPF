using CommunityToolkit.Mvvm.ComponentModel;
using SDIFrontEnd_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;
using MvvmLib.ViewModels;
using CommunityToolkit.Mvvm.Input;
namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class PraccingImportItemViewModel : ViewModelBase
    {
        public PraccingIssue Model { get; }
        public int ID { get; }

        [ObservableProperty]
        private int issueNo;

        [ObservableProperty]
        private string varNames = string.Empty;

        [ObservableProperty]
        private Person? from;

        [ObservableProperty]
        private Person? to;

        [ObservableProperty]
        private PraccingCategory? category;

        [ObservableProperty]
        private DateTime? date;

        [ObservableProperty]
        private string pin = string.Empty;

        [ObservableProperty]
        private string status = string.Empty;

        [ObservableProperty]
        private bool keepIssue = true;

        [ObservableProperty]
        private bool moving;

        [ObservableProperty]
        private bool resolved;

        [ObservableProperty]
        private DateTime? resolvedDate;

        [ObservableProperty]
        private Person? resolvedBy;

        [ObservableProperty]
        private string description = string.Empty;

        public ObservableCollection<PraccingImportResponseItemViewModel> ExistingResponses { get; } = [];

        public ObservableCollection<PraccingImportResponseItemViewModel> NewResponses { get; } = [];


        public bool HasOldResponses { get => ExistingResponses.Count > 0; }
        public bool HasNewResponses { get => NewResponses.Count > 0; }
        public bool HasImages { get => Model.Images.Count > 0; }

        public PraccingImportItemViewModel(PraccingIssue model)
        {
            Model = model;
            ID = model.ID;
            IssueNo = model.IssueNo;
            VarNames = model.VarNames;
            From = model.IssueFrom;
            To = model.IssueTo;
            Category = model.Category;
            Date = model.IssueDate;
            Pin = model.PinNo;
            
            Resolved = model.Resolved;
            ResolvedDate = model.ResolvedDate;
            ResolvedBy = model.ResolvedBy;
            Description = model.Description;

            foreach (PraccingResponse response in model.Responses)
            {
                if (response.ID <= 0)
                    NewResponses.Add(new PraccingImportResponseItemViewModel(response) { NewResponse = true});
                else
                    ExistingResponses.Add(new PraccingImportResponseItemViewModel(response));
            }
            OnPropertyChanged(nameof(HasNewResponses));
            OnPropertyChanged(nameof(HasOldResponses));
        }


    }


    
}
