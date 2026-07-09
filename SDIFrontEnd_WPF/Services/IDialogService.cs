using ITCLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using MvvmLib;
using MvvmLib.ViewModels;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
namespace SDIFrontEnd_WPF
{
    public interface IDialogService
    {
        string OpenFile(string filter);
        string OpenSurveyImageFile();
        void ShowMessage(string message, string title = "Info");
        void ShowError(string message, string title = "Error");
        bool Confirm(string message, string title = "Confirm");
        string PromptForText(string message, string title = "Input");

        SurveyQuestion PickQuestion(IEnumerable<SurveyQuestion> candidates);
        bool? ShowDialog(WorkspaceViewModel viewModel);
        Task<bool?> ShowDialogAsync<TViewModel>(Func<TViewModel, Task>? configure = null) where TViewModel : WorkspaceViewModel;
        void ShowWindow(WorkspaceViewModel viewModel);
    }

    public class  DialogService : IDialogService 
    {
        private readonly IServiceProvider _serviceProvider;

        public DialogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public string OpenFile(string filter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = filter,
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }

        public string OpenSurveyImageFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = @"\\psychfile\psych$\psych-lab-gfong\SMG\Survey Images";
            dialog.Filter = "Images|*.jpg;*.jpeg;*.png";
            dialog.ShowDialog();

            string file = dialog.FileName;

            if (string.IsNullOrEmpty(file))
            {
                return string.Empty;
            }
            else if (!file.StartsWith(@"\\psychfile\psych$\psych-lab-gfong\SMG\Survey Images"))
            {
                MessageBox.Show(@"Images must be stored in the following folder: \r\n" +
                    @"\\psychfile\psych$\psych-lab-gfong\SMG\Survey Images\[Project Code] Images\[Survey Code]. Ensure the file exists and try again.");

                return string.Empty;
            }
            return file;
        }

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

        public async Task<bool?> ShowDialogAsync<TViewModel>(Func<TViewModel, Task>? configure = null) where TViewModel : WorkspaceViewModel
        {
            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

            if (configure != null)
                await configure(viewModel);

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

        public void ShowWindow(WorkspaceViewModel viewModel)
        {
            var window = new Window
            {
                Title = viewModel.DisplayName ?? string.Empty,
                SizeToContent = SizeToContent.WidthAndHeight,
                Content = new ContentControl { Content = viewModel },
                DataContext = viewModel,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
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

            window.Show();
        }
    }
}