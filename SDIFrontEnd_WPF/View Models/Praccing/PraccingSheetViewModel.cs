using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmLib.ViewModels;
using ITCLib;
using ITCReportLib;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class PraccingSheetViewModel : ViewModelBase
    {
        private readonly IApiSurveyService _surveyService;      

        public List<Survey> SurveyList { get; set; } = new List<Survey>();
        public bool UseWord { get; set; } = true;
        public bool UseExcel { get; set; } = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateSheetCommand))]
        private Survey? selectedSurvey;

        public PraccingSheetViewModel(IApiSurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        public async Task LoadSurveys()
        {
            
            SurveyList = (await _surveyService.GetAllAsync()).ToList();
            OnPropertyChanged(nameof(SurveyList));
        }

        [RelayCommand(CanExecute = nameof(CanRun))]
        private async Task CreateSheet()
        {
            var questions = await _surveyService.GetSurveyQuestions(SelectedSurvey.SID);
            SelectedSurvey.Questions.Clear();
            SelectedSurvey.AddQuestions(questions);

            PraccingSheetGenerator generator = new PraccingSheetGenerator(SelectedSurvey);
            generator.UseWord = UseWord;
            generator.Print();
        }

        public bool CanRun() => SelectedSurvey != null;
    }
}
