using CommunityToolkit.Mvvm.Input;
using ITCLib;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITC_Services;
using CommunityToolkit.Mvvm.ComponentModel;
namespace SDIFrontEnd_WPF
{
    public partial class QuestionSearchViewModel : WorkspaceViewModel
    {
        private readonly IApiSurveyService _surveyService;
        private readonly IApiQuestionService _questionService;

        public ObservableCollection<SurveyQuestion> SearchResults { get; set; } = new ObservableCollection<SurveyQuestion>();
        public ObservableCollection<SurveyQuestion> FilteredResults { get; set; } = new ObservableCollection<SurveyQuestion>();
        public string SearchTerm { get; set; } = string.Empty;

        public List<Survey> SurveyList { get; private set; }

        [ObservableProperty]
        private bool searchPreP;
        [ObservableProperty]
        private bool searchPreI; 
        [ObservableProperty]
        private bool searchPreA = true;
        [ObservableProperty]
        private bool searchLitQ = true;
        [ObservableProperty]
        private bool searchPstI;
        [ObservableProperty]
        private bool searchPstP;
        [ObservableProperty]
        private bool searchRO = true;
        [ObservableProperty]
        private bool searchNR;

        public QuestionSearchViewModel(IApiSurveyService surveyService, IApiQuestionService questionService)
        {
            _surveyService = surveyService;
            this.DisplayName = "Question Search";
            _questionService = questionService;
            _ = LoadSurveys();
        }

        private async Task LoadSurveys()
        {
            SurveyList = await _surveyService.GetAllAsync();
            OnPropertyChanged(nameof(SurveyList));
        }

        [RelayCommand]
        private async Task Search()
        {
            // TODO survey and varname are not used in the search, need to add those as parameters to the API and filter on the backend

            SearchResults.Clear();
            SearchResults = new ObservableCollection<SurveyQuestion>( await _questionService.SearchQuestions(SearchTerm));
            FilteredResults = new ObservableCollection<SurveyQuestion>(UpdateFilter());
        }

       
        private List<SurveyQuestion> UpdateFilter()
        {
            var filtered = new List<SurveyQuestion>();
            if (SearchPreP)
            {
                filtered.AddRange(SearchResults.Where(q => q.PrePW.WordingText.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }
            if (SearchPreI)
            {
                filtered.AddRange(SearchResults.Where(q => q.PreIW.WordingText.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (SearchPreA)
            {
                filtered.AddRange(SearchResults.Where(q => q.PreAW.WordingText.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (SearchLitQ)
            {
                filtered.AddRange(SearchResults.Where(q => q.LitQW.WordingText.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (SearchPstI)
            {
                filtered.AddRange(SearchResults.Where(q => q.PstIW.WordingText.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (SearchPstP)
            {
                filtered.AddRange(SearchResults.Where(q => q.PstPW.WordingText.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (SearchRO)
            {
                filtered.AddRange(SearchResults.Where(q => q.RespOptionsS.RespList.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            if (SearchNR)
            {
                filtered.AddRange(SearchResults.Where(q => q.NRCodesS.RespList.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            return filtered;
        }
    }
}
