using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using SDIFrontEnd_WPF.Views;
using SDIFrontEnd_WPF.ViewModels;
using MvvmLib.ViewModels;

namespace SDIFrontEnd_WPF
{
    public class WindowService : IWindowService
    {
        private readonly IServiceProvider _serviceProvider;

        public WindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowQuestionImporterWindow()
        {
            var viewModel = _serviceProvider.GetRequiredService<QuestionImporterViewModel>();
            var window = new QuestionImporterWindow
            {
                DataContext = viewModel
            };         

            window.Show();
        }

        public void ShowSearchWindow(List<PraccingIssue> issues)
        {
            var viewModel = new BrowseIssuesViewModel(issues);
            var window = new BrowseWindow
            {
                DataContext = viewModel
            };

            // Allow the ViewModel to close the window
            viewModel.CloseWindow = () => window.Close();

            window.ShowDialog(); 
        }
        
        public void ShowImageWindow(PraccingImage image)
        {
            //List<PraccingImage> images = new List<PraccingImage>() { image };
            //var viewModel = new PraccingImagesViewModel(new ObservableCollection<PraccingImage>(images));
            //var window = new ImageWindow
            //{
            //    DataContext = viewModel
            //};
            //// Allow the ViewModel to close the window
            //viewModel.CloseWindow = () => window.Close();
            //window.ShowDialog();
        }
    }
}
