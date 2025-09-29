using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ITCLib;
using MvvmLib.ViewModels;
using MvvmLib;
namespace SDIFrontEnd_WPF
{
    public interface IDialogService
    {
        void ShowMessage(string message, string title = "Info");
        void ShowError(string message, string title = "Error");
        bool Confirm(string message, string title = "Confirm");
        string PromptForText(string message, string title = "Input");

        SurveyQuestion PickQuestion(IEnumerable<SurveyQuestion> candidates);
        bool? ShowDialog(WorkspaceViewModel viewModel);
    }

    public class  DialogService : IDialogService 
    {
        public void ShowMessage(string message, string title = "Info")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public bool Confirm(string message, string title = "Confirm")
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public string PromptForText(string message, string title = "Input")
        {
            var dialog = new PromptDialog(message, title); // Custom Input Dialog
            if (dialog.ShowDialog() == true)
            {
                return dialog.ResponseText;
            }
            return null;
        }

        public SurveyQuestion PickQuestion(IEnumerable<SurveyQuestion> candidates)
        {
            var dlg = new PickQuestionDialog(candidates);
            if (dlg.ShowDialog() == true)
                return dlg.SelectedQuestion;
            return null;
        }

        public bool? ShowDialog(WorkspaceViewModel viewModel)
        {
            var window = new Window
            {
                Title = viewModel.DisplayName ?? string.Empty,
                SizeToContent = SizeToContent.WidthAndHeight,
                Content = new ContentControl { Content = viewModel },
                DataContext = viewModel,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner

            };

            // Keep a strong reference to the handler so we can unsubscribe
            EventHandler<DialogResultEventArgs>? handler = null;

            handler = (s, args) =>
            {
                // Detach to break the ViewModel → Window reference chain
                viewModel.RequestClose -= handler;

                // If you use a custom EventArgs with a DialogResult
                window.DialogResult = args.DialogResult;
                window.Close();
            };

            viewModel.RequestClose += handler;

            // Clean up after the dialog closes (no matter how it closes)
            window.Closed += (s, e) =>
            {
                // Dispose if the VM implements IDisposable
                if (viewModel is IDisposable disposable)
                    disposable.Dispose();
            };

            //viewModel.RequestClose += (s, args) =>
            //{
            //    window.DialogResult = args.DialogResult; // or false based on your logic
            //    window.Close();
            //};

            return window.ShowDialog();
        }
    }
}