using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmLib.ViewModels;
using ITCLib;
using CommunityToolkit.Mvvm.ComponentModel;
namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class MatrixColumnViewModel : ViewModelBase
    {

        public MatrixColumn Column { get; }

        public string Header => Column.Header;

        [ObservableProperty]
        private VariableGridOptions options;

        [ObservableProperty]
        private bool isPopupOpen;

        public bool IsSurvey => Column.Type == MatrixColumnType.Survey;

        public MatrixColumnViewModel(MatrixColumn column)
        {
            Column = column;
            Options = new VariableGridOptions();
            Options.PropertyChanged += (s, e) =>
            {
                OnSurveyColumnOptionChanged?.Invoke(this, e.PropertyName);
            };
            
        }
        public event Action<MatrixColumnViewModel, string?>? OnSurveyColumnOptionChanged;
    }
}
