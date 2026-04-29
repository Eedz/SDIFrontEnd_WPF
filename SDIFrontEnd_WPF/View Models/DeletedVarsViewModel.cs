using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SDIFrontEnd_WPF
{
    public partial class DeletedQuestionsViewModel : WorkspaceViewModel
    {
        private readonly List<DeletedQuestion> _questions;

        public List<DeletedQuestion> Questions => _questions;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ItemPosition))]
        private DeletedQuestion? selectedQuestion;

        public string ItemPosition => $"{Questions.IndexOf(SelectedQuestion) +1} of {Questions.Count}";

        public DeletedQuestionsViewModel(List<DeletedQuestion> questions)
        {
            DisplayName = "Deleted Questions";
            _questions = questions ?? throw new ArgumentNullException(nameof(questions));
            SelectedQuestion = _questions.FirstOrDefault();
        }

        [RelayCommand]
        private void FirstItem()
        {
            if (Questions.Count > 0)
                SelectedQuestion = Questions.FirstOrDefault();
        }

        [RelayCommand]
        private void LastItem() 
        {
            if (Questions.Count > 0)
                SelectedQuestion = Questions.LastOrDefault();
        }

        [RelayCommand]
        private void PreviousItem()
        {
            if (Questions == null)
                return;
            if (SelectedQuestion == null)
            {
                SelectedQuestion = Questions.FirstOrDefault();
                return;
            }
            int currentIndex = Questions.IndexOf(SelectedQuestion);
            if (currentIndex > 0)
            {
                SelectedQuestion = Questions[currentIndex - 1];
            }
            else
            {
                SelectedQuestion = Questions.Last();
            }
        }

        [RelayCommand]
        private void NextItem()
        {
            if (Questions == null)
                return;
            if (SelectedQuestion == null)
            {
                SelectedQuestion = Questions.First();
                return;
            }
            int currentIndex = Questions.IndexOf(SelectedQuestion);
            if (currentIndex < Questions.Count - 1)
            {
                SelectedQuestion = Questions[currentIndex + 1];
            }
            else
            {
                SelectedQuestion = Questions.First();
            }
        }

    }
}
