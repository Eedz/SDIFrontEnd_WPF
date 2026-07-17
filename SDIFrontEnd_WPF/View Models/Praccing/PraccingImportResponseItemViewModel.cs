using CommunityToolkit.Mvvm.ComponentModel;
using ITCLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using MvvmLib.ViewModels;
using System.Windows.Markup;

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class PraccingImportResponseItemViewModel : ViewModelBase
    {
        private readonly PraccingResponse _response;
        public PraccingResponse Response => _response;

        [ObservableProperty]
        private string responseText = string.Empty;

        public int ImageCount => _response.Images?.Count ?? 0;

        [ObservableProperty]
        private bool keepResponse = true;

        [ObservableProperty]
        private bool newResponse = false;

        public PraccingImportResponseItemViewModel(PraccingResponse response)
        {
            _response = response;

          
         
            ResponseText = response.Response;
        }

        public void SetResponse(PraccingResponse response)
        {
            if (response == null) return;
            _response.Response = response.Response;
        
         
            ResponseText = response.Response;
            OnPropertyChanged(nameof(Response));
        }
    }
}
