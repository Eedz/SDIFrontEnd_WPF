using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITC_Services;
using MvvmLib.ViewModels;
using ITCLib;
using ITCReportLib;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class PraccingSheetViewModel : ViewModelBase
    {
        private readonly ISurveyService _surveyService;      

        public List<Survey> SurveyList { get; set; }
        public bool UseWord { get; set; } = true;
        public bool UseExcel { get; set; } = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateSheetCommand))]
        private Survey selectedSurvey;

        public PraccingSheetViewModel(ISurveyService surveyService)
        {
            _surveyService = surveyService;
            SurveyList = _surveyService.GetAllSurveys().ToList();
        }

        [RelayCommand(CanExecute = nameof(CanRun))]
        private void CreateSheet()
        {
            SelectedSurvey.AddQuestions(_surveyService.GetQuestionsForSurvey(SelectedSurvey.SID));

            PraccingSheetGenerator generator = new PraccingSheetGenerator(SelectedSurvey);
            generator.UseWord = UseWord;
            generator.Print();
        }

        public bool CanRun() => SelectedSurvey != null;
    }
}
