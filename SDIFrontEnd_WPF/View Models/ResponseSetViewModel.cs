using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITC_Services;
using ITCLib;
using MvvmLib.ViewModels;
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
        private readonly IWordingService _wordingService; // Service for managing question wordings and translations

        public bool NewSet { get; set; }
        public string ResponseType { get; set; } // Type of wording being managed (e.g., PreP, PreI etc.)
        [ObservableProperty]
        private ResponseSet? currentResponse;
        public ObservableCollection<ResponseSet> Responses { get; set; } = new ObservableCollection<ResponseSet>(); // Collection of wordings for the UI
        [ObservableProperty]
        private ObservableCollection<ResponseUsage> usages = new ObservableCollection<ResponseUsage>(); // Collection of wordings for the UI

        [ObservableProperty]
        private bool lockedForEditing = true; // Flag indicating if the wording is locked for editing

        public string ItemPosition => $"{(Responses.IndexOf(CurrentResponse) + 1)} of {Responses.Count}";

        public ResponseSetViewModel(IWordingService wordingService, IDialogService dialogService, string type)
        {
            DisplayName = "Wording Manager";
            _wordingService = wordingService ?? throw new ArgumentNullException(nameof(wordingService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            ResponseType = type ?? throw new ArgumentNullException(nameof(type));
            switch (type)
            {
                case "RespOptions": Responses = new ObservableCollection<ResponseSet>( _wordingService.GetAllResponseSets()); break;
                case "NRCodes": Responses = new ObservableCollection<ResponseSet>(_wordingService.GetAllNonResponseSets()); break;
            }
            CurrentResponse = Responses.FirstOrDefault();
        }

        public ResponseSetViewModel(IWordingService wordingService, IDialogService dialogService, string type, string wordID) : this(wordingService, dialogService, type)
        {
            CurrentResponse = Responses.FirstOrDefault(w => w.RespSetName == wordID);
        }

        partial void OnCurrentResponseChanged(ResponseSet? value)
        {
            if (value.RespSetName == "0")
            {
                Usages.Clear();
                return;
            }

            Usages = new ObservableCollection<ResponseUsage>(_wordingService.GetResponseUsages(ResponseType, CurrentResponse.RespSetName));
            NewSet = CurrentResponse.RespSetName == string.Empty;
        }

        [RelayCommand]
        private void EditResponseSet()
        {
            if (!LockedForEditing)
            {
                // save changes
                if (string.IsNullOrEmpty(CurrentResponse.RespSetName))
                    return;
                //_wordingService.InsertResponseSet(CurrentResponse);
                else

                    _wordingService.UpdateResponseSet(CurrentResponse);
            }
            LockedForEditing = !LockedForEditing;
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
            Responses.Add(newWording);
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
            Responses.Add(newWording);
        }

        [RelayCommand]
        private void DeleteWording(ResponseSet wording)
        {
            //if (wording.WordID != 0)
            //    _wordingService.DeleteWording(wording);
            Responses.Remove(wording);
            CurrentResponse = Responses.FirstOrDefault();
        }

        [RelayCommand]
        private void ClipSearch()
        {
            // TODO use clipboard service
            string criteria = _dialogService.PromptForText("Enter search text", "Search");
            var results = Responses.Where(x => x.RespList.Contains(criteria));
            Responses = new ObservableCollection<ResponseSet>(results);
            CurrentResponse = Responses.FirstOrDefault();
        }

        [RelayCommand]
        private void TextSearch()
        {
            string criteria = _dialogService.PromptForText("Enter search text", "Search");
            var results = Responses.Where(x => x.RespList.Contains(criteria));
            Responses = new ObservableCollection<ResponseSet>(results);
            CurrentResponse = Responses.FirstOrDefault();
        }

        [RelayCommand]
        private void SelectWording()
        {
            if (!LockedForEditing || (CurrentResponse != null && CurrentResponse.RespSetName != "0"))
            {
                if (_dialogService.Confirm("You have unsaved changes. Save first?"))
                    return;
            }
            else
            {
                CloseCommand.Execute(true);
            }
        }

        [RelayCommand]
        private void PreviousItem()
        {
            if (Responses == null)
                return;
            if (CurrentResponse == null)
            {
                CurrentResponse = Responses.FirstOrDefault();
                return;
            }
            int currentIndex = Responses.IndexOf(CurrentResponse);
            if (currentIndex > 0)
            {
                CurrentResponse = Responses[currentIndex - 1];
            }
            else
            {
                CurrentResponse = Responses.Last();
            }
        }

        [RelayCommand]
        private void NextImage()
        {
            if (Responses == null)
                return;
            if (CurrentResponse == null)
            {
                CurrentResponse = Responses.First();
                return;
            }
            int currentIndex = Responses.IndexOf(CurrentResponse);
            if (currentIndex < Responses.Count - 1)
            {
                CurrentResponse = Responses[currentIndex + 1];
            }
            else
            {
                CurrentResponse = Responses.First();
            }
        }

        private ITCLib.ResponseType GetResponseType()
        {
            switch (ResponseType)
            {
                case "RespOptions": return ITCLib.ResponseType.RespOptions;
                case "NRCodes": return ITCLib.ResponseType.NRCodes;
                default: throw new ArgumentException("Invalid Wording Type");
            }
        }
    }
}
