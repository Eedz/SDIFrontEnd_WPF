using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Wordprocessing;
using ITC_Services;
using ITCLib;
using Microsoft.Extensions.DependencyInjection;
using MvvmLib.ViewModels;

namespace SDIFrontEnd_WPF
{
    public partial class EditWordingViewModel : ViewModelBase
    {
        private readonly IWordingService _wordingService;

        [ObservableProperty]
        private string newWordingText;

        public WordingType NewWordingType;

        public List<WordingUsage> WordingUses;

        partial void OnNewWordingTextChanged(string value)
        {
            UpdateUses(value);
        }
        

        public EditWordingViewModel(IWordingService wordingService )
        {
            _wordingService = wordingService ?? throw new ArgumentNullException(nameof(wordingService));
            WordingUses = new List<WordingUsage>();
        }

        [RelayCommand]
        private void UpdateUses(string wordingText)
        {
            Wording newWording = new Wording
            {
                WordingText = wordingText,
                Type = NewWordingType
            };

            WordingUses = _wordingService.GetWordingUsages(newWording);
        }
    }
}
