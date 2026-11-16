using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using ITCLib;
using MvvmLib.ViewModels;
namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class NewSurveyViewModel : WorkspaceViewModel
    {
		private readonly IApiSurveyService _surveyService;
		private readonly ReferenceDataStore _referenceService;

		public List<StudyWave> StudyWaves { get; private set; } = new List<StudyWave>();
		public List<SurveyCohort> SurveyCohorts { get; private set; } = new List<SurveyCohort>();
		public List<SurveyMode> SurveyModes { get; private set; } = new List<SurveyMode>();
		public List<Survey> Surveys { get; private set; } = new List<Survey>();

		public Survey NewSurvey {get;set; } = new Survey();
		public Survey CopyFrom { get; set; }

		public NewSurveyViewModel(IApiSurveyService surveyService, ReferenceDataStore referenceService)
		{
			DisplayName = "New Survey";
			_surveyService = surveyService;
			_referenceService = referenceService;
		}

		public async Task Load()
		{
			StudyWaves = await _surveyService.GetAllWavesAsync();
			SurveyCohorts = _referenceService.Cohorts.ToList() ;
			SurveyModes = _referenceService.Modes.ToList();
			Surveys = await _surveyService.GetAllAsync();
		}

		[RelayCommand]
		private async Task CreateSurvey()
		{
			var result = await _surveyService.CreateSurveyAsync(NewSurvey);
			NewSurvey = result;				
			OnRequestClose(true);

		}

		[RelayCommand]
		private void Cancel()
		{
			OnRequestClose(false);
		}
	}
}
