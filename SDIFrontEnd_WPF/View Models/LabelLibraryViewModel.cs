using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using ITCLib;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class LabelLibraryViewModel : WorkspaceViewModel
    {
        private readonly ReferenceDataStore _referenceDataStore;
        private readonly IApiLabelService _labelService;
        private readonly IApiQuestionService _surveyService;

        public List<string> LabelTypes { get; set; } = new List<string>() { "Content", "Topic", "Domain", "Product" };
        [ObservableProperty]
        private string? selectedLabelType;

        public List<VarNameLabel> Labels { get; set; } = new List<VarNameLabel>();
        [ObservableProperty]
        private VarNameLabel? selectedLabel;

        public List<VariableName> VarNames { get; set; } = new List<VariableName>();
        [ObservableProperty]
        private VariableName? selectedVarName;

        public List<SurveyQuestion> Questions { get; set; } = new List<SurveyQuestion>();

        public LabelLibraryViewModel(ReferenceDataStore referenceDataStore, IApiLabelService labelService, IApiQuestionService surveyService)
        {
            this.DisplayName = "Label Library";
            _referenceDataStore = referenceDataStore;
            _labelService = labelService;
            _surveyService = surveyService;
        }

        partial void OnSelectedLabelTypeChanged(string value)
        {
            switch (value.ToLower())
            {
                case "content":
                    Labels = _referenceDataStore.ContentLabels.OrderBy(l => l.Label).ToList();
                    break;
                case "topic":
                    Labels = _referenceDataStore.TopicLabels.OrderBy(l => l.Label).ToList();
                    break;
                case "domain":
                    Labels = _referenceDataStore.DomainLabels.OrderBy(l => l.Label).ToList();
                    break;
                case "product":
                    Labels = _referenceDataStore.ProductLabels.OrderBy(l => l.Label).ToList();
                    break;
            }
            OnPropertyChanged(nameof(Labels));
        }

        async partial void OnSelectedLabelChanged(VarNameLabel? value)
        {
            if (value != null)
            {
                VarNames = await _labelService.GetVarNamesByLabel(SelectedLabelType, value);
            }
            else
            {
                VarNames.Clear();
            }
            OnPropertyChanged(nameof(VarNames));
        }

        async partial void OnSelectedVarNameChanged(VariableName? value) 
        {
            if (value != null)
            {
                Questions = await _surveyService.GetQuestionsByVarNameAsync(value.VarName);
            }
            else
            {
                Questions.Clear();
            }
            OnPropertyChanged(nameof(Questions));
        }
    }
    
}
