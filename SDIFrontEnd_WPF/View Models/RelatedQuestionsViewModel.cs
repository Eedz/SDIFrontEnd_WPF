using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITCLib;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    // TODO styling
    public partial class RelatedQuestionsViewModel : WorkspaceViewModel
    {
        private List<SurveyQuestion> _questions;

        public List<SurveyQuestion> Questions => _questions;

        public List<SurveyQuestion> FilteredQuestions { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ItemPosition))]
        [NotifyPropertyChangedFor(nameof(CurrentQuestionText))]
        private SurveyQuestion? selectedQuestion;

        public string CurrentQuestionText => SelectedQuestion?.GetQuestionTextHTML() ?? string.Empty;

        public string ItemPosition => $"{FilteredQuestions.IndexOf(SelectedQuestion) + 1} of {FilteredQuestions.Count}";

        public List<string> CountryList => Questions
                .Select(x => Regex.Match(x.SurveyCode, @"^[A-Za-z]+").Value) // extract letters at start
                .Where(code => !string.IsNullOrEmpty(code))
                .Distinct()
                .OrderBy(x => x)
                .ToList();


        private string countryFilter;
        public string CountryFilter { get => countryFilter; set
            {
                SetProperty(ref countryFilter, value);
                if (!string.IsNullOrEmpty(value))
                {
                    FilteredQuestions = Questions.Where(x => x.SurveyCode.StartsWith(countryFilter)).ToList();
                }
                else
                {
                    FilteredQuestions = Questions;
                }

                SelectedQuestion = FilteredQuestions.FirstOrDefault();
                OnPropertyChanged(nameof(FilteredQuestions));
                OnPropertyChanged(nameof(SelectedQuestion));
                OnPropertyChanged(nameof(ItemPosition));
            }
        }

        public RelatedQuestionsViewModel(List<SurveyQuestion> questions)
        {
            base.DisplayName = "Related Questions";
            _questions = questions ?? throw new ArgumentNullException(nameof(questions));
            selectedQuestion = _questions.FirstOrDefault();
            FilteredQuestions = Questions;
            CountryFilter = string.Empty;
        }

        public RelatedQuestionsViewModel(List<SurveyQuestion> questions, string countryFilter)
        {
            base.DisplayName = "Related Questions";
            _questions = questions ?? throw new ArgumentNullException(nameof(questions));
            

            if (!string.IsNullOrEmpty(countryFilter))
            {
                CountryFilter = countryFilter;
                FilteredQuestions = _questions.Where(x => x.SurveyCode.StartsWith(countryFilter)).ToList();
                SelectedQuestion = FilteredQuestions.FirstOrDefault();
            }else 
                FilteredQuestions = Questions;

            OnPropertyChanged(nameof(FilteredQuestions));
        }

        public void UpdateQuestions(List<SurveyQuestion> question)
        {
            _questions = question;
            selectedQuestion = null;
            selectedQuestion = _questions.FirstOrDefault();

            FilteredQuestions = Questions;

            OnPropertyChanged(nameof(SelectedQuestion));
            OnPropertyChanged(nameof(Questions));
            OnPropertyChanged(nameof(CountryList));
            OnPropertyChanged(nameof(ItemPosition));
        }

        public void UpdateQuestions(List<SurveyQuestion> question, string countryFilter)
        {
            _questions = question;
            selectedQuestion = null;
            //selectedQuestion = _questions.FirstOrDefault();
            if (!string.IsNullOrEmpty(countryFilter))
            {
                CountryFilter = countryFilter;
                FilteredQuestions = _questions.Where(x => x.SurveyCode.StartsWith(countryFilter)).ToList();
                SelectedQuestion = FilteredQuestions.FirstOrDefault();
            }

            OnPropertyChanged(nameof(SelectedQuestion));
            OnPropertyChanged(nameof(Questions));
            OnPropertyChanged(nameof(CountryList));
            OnPropertyChanged(nameof(ItemPosition));
        }

        [RelayCommand]
        private void FirstItem()
        {
            if (FilteredQuestions.Count > 0)
                SelectedQuestion = FilteredQuestions.FirstOrDefault();
        }

        [RelayCommand]
        private void LastItem()
        {
            if (FilteredQuestions.Count > 0)
                SelectedQuestion = FilteredQuestions.LastOrDefault();
        }

        [RelayCommand]
        private void PreviousItem()
        {
            if (FilteredQuestions == null)
                return;
            if (SelectedQuestion == null)
            {
                SelectedQuestion = FilteredQuestions.FirstOrDefault();
                return;
            }
            int currentIndex = FilteredQuestions.IndexOf(SelectedQuestion);
            if (currentIndex > 0)
            {
                SelectedQuestion = FilteredQuestions[currentIndex - 1];
            }
            else
            {
                SelectedQuestion = FilteredQuestions.Last();
            }
        }

        [RelayCommand]
        private void NextItem()
        {
            if (FilteredQuestions == null)
                return;
            if (SelectedQuestion == null)
            {
                SelectedQuestion = FilteredQuestions.First();
                return;
            }
            int currentIndex = FilteredQuestions.IndexOf(SelectedQuestion);
            if (currentIndex < FilteredQuestions.Count - 1)
            {
                SelectedQuestion = FilteredQuestions[currentIndex + 1];
            }
            else
            {
                SelectedQuestion = FilteredQuestions.First();
            }
        }
    }
}
