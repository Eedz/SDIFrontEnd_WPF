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

        public ObservableCollection<Translation> Translations { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentTranslationText))]
        [NotifyPropertyChangedFor(nameof(ItemPosition))]
        private Translation? currentTranslation;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EnglishPreP))]
        [NotifyPropertyChangedFor(nameof(EnglishPstP))]
        [NotifyPropertyChangedFor(nameof(CurrentTranslationText))]
        private SurveyQuestion question;


        private FlowDocument currentTranslationText;
        public FlowDocument CurrentTranslationText
        {
            get => currentTranslationText;
            set
            {
                SetProperty(ref currentTranslationText, value);
                if (CurrentTranslation != null)
                    CurrentTranslation.TranslationText = HtmlUtils.ConvertFlowDocumentToHtml(value);
            }
        }

        public string ItemPosition => $"{(Translations.IndexOf(CurrentTranslation) + 1)} of {Translations.Count}";
        public string EnglishPreP => question.PrePW.WordingText ?? string.Empty;
        public string EnglishPstP => question.PstPW.WordingText ?? string.Empty;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="question"></param>
        public TranslationViewModel(SurveyQuestion question)
        {
            base.DisplayName = "Translations";
            Question = question;
            Translations = new ObservableCollection<Translation>(question.Translations);
            CurrentTranslation = Translations.FirstOrDefault();
            if (CurrentTranslation!=null)
                CurrentTranslationText = (FlowDocument)XamlReader.Parse(HtmlToXaml.HtmlToXamlConverter.ConvertHtmlToXaml(CurrentTranslation.TranslationText, true));
            
        }

        partial void OnQuestionChanged(SurveyQuestion value)
        {
            Translations = new ObservableCollection<Translation>(value.Translations);
            CurrentTranslation = Translations.FirstOrDefault();
            if (CurrentTranslation != null)
                CurrentTranslationText = (FlowDocument)XamlReader.Parse(HtmlToXaml.HtmlToXamlConverter.ConvertHtmlToXaml(CurrentTranslation.TranslationText, true));
            else
                CurrentTranslationText = new FlowDocument();
            OnPropertyChanged(nameof(CurrentTranslationText));
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
        private void Save()
        {

        }

        public void UpdateTranslations(SurveyQuestion question)
        {
            Question = question;
            
        }

    }
}
