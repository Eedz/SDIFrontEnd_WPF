using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITCLib;
using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Markup;

namespace SDIFrontEnd_WPF
{
    // TODO Save command
    // TODO colors
    public partial class TranslationViewModel : WorkspaceViewModel
    {
        private readonly IApiQuestionService _questionService;
        public ObservableCollection<Translation> Translations { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TranslationText))]
        [NotifyPropertyChangedFor(nameof(ItemPosition))]
        private Translation? currentTranslation;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EnglishPreP))]
        [NotifyPropertyChangedFor(nameof(EnglishPstP))]
        [NotifyPropertyChangedFor(nameof(TranslationText))]
        private SurveyQuestion question;


        [ObservableProperty]
        private string translationText;

        public string ItemPosition => $"{(Translations.IndexOf(CurrentTranslation) + 1)} of {Translations.Count}";

        public string? EnglishPreP { get;  set; }         
        public string? EnglishPstP { get;  set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="question"></param>
        public TranslationViewModel(IApiQuestionService service, SurveyQuestion question)
        {
            base.DisplayName = "Translations";
            _questionService = service;
            Question = question;           
        }

        partial void OnQuestionChanged(SurveyQuestion value)
        {
            EnglishPreP = value?.PrePW?.WordingText;
            EnglishPstP = value?.PstPW?.WordingText;

            Translations = new ObservableCollection<Translation>(value.Translations);
            CurrentTranslation = Translations.FirstOrDefault();
            if (CurrentTranslation != null)
                //CurrentTranslationText = (FlowDocument)SimpleHtmlConverter.FromHtml(CurrentTranslation.TranslationText);
                TranslationText = CurrentTranslation.TranslationText;
            else
                //CurrentTranslationText = new FlowDocument();
                TranslationText = string.Empty;
            OnPropertyChanged(nameof(TranslationText));
        }

        partial void OnCurrentTranslationChanged(Translation? oldValue, Translation? newValue)
        {
            if (CurrentTranslation != null)
                TranslationText = CurrentTranslation.TranslationText;
            else
                TranslationText = string.Empty;
            OnPropertyChanged(nameof(TranslationText));
        }

        [RelayCommand]
        private void PreviousItem()
        {
            if (Translations == null)
                return;
            if (CurrentTranslation == null)
            {
                CurrentTranslation = Translations.FirstOrDefault();
                return;
            }
            int currentIndex = Translations.IndexOf(CurrentTranslation);
            if (currentIndex > 0)
            {
                CurrentTranslation = Translations[currentIndex - 1];
            }
            else
            {
                CurrentTranslation = Translations.Last();
            }
        }

        [RelayCommand]
        private void NextItem()
        {
            if (Translations == null)
                return;
            if (CurrentTranslation == null)
            {
                CurrentTranslation = Translations.First();
                return;
            }
            int currentIndex = Translations.IndexOf(CurrentTranslation);
            if (currentIndex < Translations.Count - 1)
            {
                CurrentTranslation = Translations[currentIndex + 1];
            }
            else
            {
                CurrentTranslation = Translations.First();
            }
        }

        [RelayCommand]
        private void FirstItem()
        {
            if (Translations == null)
                return;

            CurrentTranslation = Translations.FirstOrDefault();
        }

        [RelayCommand]
        private void LastItem()
        {
            if (Translations == null)
                return;

            CurrentTranslation = Translations.LastOrDefault();
        }

        [RelayCommand]
        private async Task Save()
        {
            if (CurrentTranslation == null) return;
            if (CurrentTranslation.ID == 0)

                await _questionService.CreateTranslation(CurrentTranslation);
            else 
                await _questionService.UpdateTranslation(CurrentTranslation);
        }

        public void UpdateTranslations(SurveyQuestion question)
        {
            Question = question;
        }

    }
}
