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

        private FlowDocument responseFlow;
        public FlowDocument ResponseFlow
        {
            get => responseFlow;
            set
            {
                SetProperty(ref responseFlow, value);
                _response.Response = HtmlUtils.ConvertFlowDocumentToHtml(responseFlow);
            }
        }

        public int ImageCount => ImagesVM.Images?.Count ?? 0;

        public PraccingResponseViewModel(PraccingResponse response)
        {
            _response = response;

            ImagesVM = new PraccingImagesViewModel(new ObservableCollection<PraccingImage>(response.Images));
            responseFlow = (FlowDocument)XamlReader.Parse(HtmlToXaml.HtmlToXamlConverter.ConvertHtmlToXaml(_response.Response, true));
        }

        public void SetResponse(PraccingResponse response)
        {
            if (response == null) return;   
            _response.Response = response.Response;
            ImagesVM.SetImages(new ObservableCollection<PraccingImage>(response.Images));
            responseFlow = (FlowDocument)XamlReader.Parse(HtmlToXaml.HtmlToXamlConverter.ConvertHtmlToXaml(_response.Response, true));
            OnPropertyChanged(nameof(Response));
        }
    }
}
