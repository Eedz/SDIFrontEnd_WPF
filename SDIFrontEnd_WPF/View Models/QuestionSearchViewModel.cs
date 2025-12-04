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
        private readonly ISurveyService _surveyService;

        public ObservableCollection<SurveyQuestion> SearchResults { get; set; } = new ObservableCollection<SurveyQuestion>();
        public ObservableCollection<SurveyQuestion> FilteredResults { get; set; } = new ObservableCollection<SurveyQuestion>();
        public string SearchTerm { get; set; } = string.Empty;

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

        public QuestionSearchViewModel(ISurveyService surveyService)
        {
            _surveyService = surveyService;
            this.DisplayName = "Question Search";
        }

        [RelayCommand]
        private void Search()
        {
            SearchResults.Clear();
            SearchResults = new ObservableCollection<SurveyQuestion>( _surveyService.SearchQuestions(SearchTerm));
            FilteredResults = new ObservableCollection<SurveyQuestion>(UpdateFilter());
           
            OnPropertyChanged(nameof(FilteredResults));
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
