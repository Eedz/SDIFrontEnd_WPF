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

namespace SDIFrontEnd_WPF
{
    public partial class SurveyEditorViewModel : WorkspaceViewModel
    {
        private readonly Survey _survey;

        public IReadOnlyList<SurveyCohort> Cohorts { get; set; }
        public IReadOnlyList<SurveyMode> Modes { get; set; }
        public IReadOnlyList<UserState> UserStates { get; set; }
        public IReadOnlyList<ScreenedProduct> ScreenedProducts { get; set; }
        public IReadOnlyList<Language> Languages { get; set; }
        public Survey Survey => _survey;
        public List<Language> LanguageList => _survey.LanguageList.Select(x => x.SurvLanguage).ToList();
        public List<UserState> UserStatesList => _survey.UserStates.Select(x => x.State).ToList();
        public List<ScreenedProduct> ScreenedProductsList => _survey.ScreenedProducts.Select(x => x.Product).ToList();

        public SurveyEditorViewModel(Survey survey, LookupProvider lookup)
        {
            DisplayName = "Survey - " + survey.SurveyCode;
            _survey = survey;
            Modes = lookup.Modes;
            UserStates = lookup.UserStates;
            ScreenedProducts = lookup.ScreenedProducts;
            Languages = lookup.Languages;
            Cohorts = lookup.Cohorts;
        }

        [RelayCommand]
        private void DeleteUserState(UserState userState)
        {
            if (userState == null) return;
            _survey.UserStates.RemoveAll(x => x.State.Equals(userState));
            OnPropertyChanged(nameof(UserStatesList));
        }

        [RelayCommand]
        private void DeleteProduct(ScreenedProduct product)
        {
            if (product == null) return;
            _survey.ScreenedProducts.RemoveAll(x => x.Product.ProductName.Equals(product));
            OnPropertyChanged(nameof(ScreenedProductsList));
        }

        [RelayCommand]
        private void DeleteLanguage(Language language)
        {
            if (language == null) return;
            _survey.LanguageList.RemoveAll(x => x.SurvLanguage.Equals(language));
            OnPropertyChanged(nameof(LanguageList));
        }

        [RelayCommand]
        private void AddUserState(UserState userState)
        {
            if (userState == null || _survey.UserStates.Any(x => x.State.Equals(userState))) return;
            _survey.UserStates.Add(new SurveyUserState { SurvID = _survey.SID, State = new UserState(userState.ID, userState.UserStateName) });
            OnPropertyChanged(nameof(UserStatesList));
        }

        [RelayCommand]
        private void AddProduct(ScreenedProduct product)
        {
            if (product == null || _survey.ScreenedProducts.Any(x => x.Product.ProductName.Equals(product.ProductName))) return;
            _survey.ScreenedProducts.Add(new SurveyScreenedProduct { SurvID = _survey.SID, Product = product });
            OnPropertyChanged(nameof(ScreenedProductsList));
        }
        [RelayCommand]
        private void AddLanguage(Language language)
        {
            if (language == null || _survey.LanguageList.Any(x => x.SurvLanguage.Equals(language))) return;
            _survey.LanguageList.Add(new SurveyLanguage { SurvID = _survey.SID, SurvLanguage = language });
            OnPropertyChanged(nameof(LanguageList));
        }
    }
}
