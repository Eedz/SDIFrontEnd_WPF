using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmLib.ViewModels;
using System.Collections.ObjectModel;
using ITCLib;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Documents;
using System.Windows.Markup;

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class PraccingResponseViewModel : ViewModelBase
    {
        private readonly PraccingResponse _response;
        public PraccingResponse Response => _response;

        public PraccingImagesViewModel ImagesVM { get; }

        [ObservableProperty]
        private string responseText = string.Empty;

        public int ImageCount => ImagesVM.Images?.Count ?? 0;

        public PraccingResponseViewModel(PraccingResponse response)
        {
            _response = response;

            ImagesVM = new PraccingImagesViewModel(new ObservableCollection<PraccingImage>(response.Images));
           
            ResponseText = response.Response;
        }

        partial void OnResponseTextChanged(string value)
        {
            _response.Response = value == null
                ? string.Empty
                : value;
        }

        public void SetResponse(PraccingResponse response)
        {
            if (response == null) return;   
            _response.Response = response.Response;
            ImagesVM.SetImages(new ObservableCollection<PraccingImage>(response.Images));
            
            ResponseText = response.Response;
            OnPropertyChanged(nameof(Response));
        }
    }
}
