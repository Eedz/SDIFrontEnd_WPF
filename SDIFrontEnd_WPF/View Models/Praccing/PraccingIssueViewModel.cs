using ITCLib;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Documents;

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class PraccingIssueViewModel : ViewModelBase
    {
        private PraccingIssueRecord _issueRecord;

        public ObservableCollection<PraccingResponseViewModel> ResponsesVM { get; }
        public PraccingImagesViewModel ImagesVM { get; }

        public PraccingIssue Issue => _issueRecord.Item;

        public bool Dirty => _issueRecord.ShouldSave;

        private FlowDocument _descriptionFlow;
        public FlowDocument DescriptionFlow
        {
            get => _descriptionFlow;
            set
            {
                SetProperty(ref _descriptionFlow, value);
               
                Issue.Description = HtmlUtils.ConvertFlowDocumentToHtml(DescriptionFlow);                
            }
        }

        public PraccingResponse CurrentResponse { get; set; }

        public PraccingIssueViewModel(PraccingIssueRecord issueRecord)
        {
            ResponsesVM = new ObservableCollection<PraccingResponseViewModel>();
            ImagesVM = new PraccingImagesViewModel();

            SetIssue(issueRecord);
        }

        public void SetIssue(PraccingIssueRecord issueRecord)
        {
            if (_issueRecord != null)
                _issueRecord.PropertyChanged -= OnRecordChanged;

            _issueRecord = issueRecord;

            if (_issueRecord != null)
                _issueRecord.PropertyChanged += OnRecordChanged;
            ResponsesVM.Clear();
            foreach (var response in issueRecord.Item.Responses)
            {
                ResponsesVM.Add(new PraccingResponseViewModel(response));
            }
            ImagesVM.SetImages(new ObservableCollection<PraccingImage>(issueRecord.Item.Images));
            DescriptionFlow = (FlowDocument)XamlReader.Parse(HtmlToXaml.HtmlToXamlConverter.ConvertHtmlToXaml(Issue.Description, true));
        }

        public void OnRecordChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PraccingIssueRecord.Dirty))
            {
                OnPropertyChanged(nameof(Dirty));
            }
        }

        public void UpdateStatus()
        {
            OnPropertyChanged(nameof(Dirty));
        }
        
    }
}
