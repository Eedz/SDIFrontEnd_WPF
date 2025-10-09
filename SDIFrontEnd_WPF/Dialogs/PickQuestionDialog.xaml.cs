using ITCLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SDIFrontEnd_WPF
{
    /// <summary>
    /// Interaction logic for PickQuestionDialog.xaml
    /// </summary>
    public partial class PickQuestionDialog : Window, INotifyPropertyChanged
    {
        private SurveyQuestion _selectedQuestion;

        public ObservableCollection<SurveyQuestion> Questions { get; }
        public SurveyQuestion SelectedQuestion
        {
            get => _selectedQuestion;
            set { _selectedQuestion = value; OnPropertyChanged(nameof(SelectedQuestion)); }
        }

        public PickQuestionDialog(IEnumerable<SurveyQuestion> candidates)
        {
            InitializeComponent();
            Questions = new ObservableCollection<SurveyQuestion>(candidates);
            DataContext = this;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedQuestion != null)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a question before proceeding.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
