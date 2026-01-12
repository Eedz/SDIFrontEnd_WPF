using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

using MvvmLib.ViewModels;
using ITCLib;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class PraccingImagesViewModel : ViewModelBase 
    {
        [ObservableProperty]
        private bool showImagePopup;

        private ObservableCollection<PraccingImage>? images;
        public ObservableCollection<PraccingImage>? Images
        {
            get => images;
            private set
            {
                SetProperty(ref images, value);
                CurrentImage = Images?.FirstOrDefault();
                OnPropertyChanged(nameof(TotalImages));
            }
        }

        
        private PraccingImage? currentImage;
        public PraccingImage? CurrentImage
        {
            get => currentImage;
            set
            {
                SetProperty(ref currentImage, value);
                OnPropertyChanged(nameof(CurrentIndex));
                NextImageCommand.NotifyCanExecuteChanged();
                PreviousImageCommand.NotifyCanExecuteChanged();
            }
        }

        [ObservableProperty]
        private int popupWidth;
        [ObservableProperty]
        private int popupHeight;

        public int TotalImages => Images?.Count ?? 0;

        
        
        public int CurrentIndex => Images.IndexOf(CurrentImage) == -1 ? 0 : Images.IndexOf(CurrentImage) + 1;


        public PraccingImagesViewModel()
        {
            Images = new ObservableCollection<PraccingImage>();
        }

        public PraccingImagesViewModel(ObservableCollection<PraccingImage> images)
        {
            Images = images;
            CurrentImage = Images.FirstOrDefault();
            PopupHeight = CurrentImage == null ? 0 : CurrentImage.Height;
            PopupWidth = CurrentImage == null ? 0 : CurrentImage.Width;
        }

        public void SetImages(ObservableCollection<PraccingImage> images)
        {
            Images = images;
            OnPropertyChanged(nameof(TotalImages));
            OnPropertyChanged(nameof(CurrentIndex));
        }

        [RelayCommand (CanExecute ="CanMovePrevious")]
        private void PreviousImage()
        {
            if (Images.Count > 0)
            {
                int index = Images.IndexOf(CurrentImage);
                if (index > 0)
                    CurrentImage = Images[index - 1];
            }
            
        }

        [RelayCommand (CanExecute ="CanMoveNext")]
        private void NextImage()
        {
            if (Images.Count > 0)
            {
                int index = Images.IndexOf(CurrentImage);
                if (index < Images.Count - 1)
                    CurrentImage = Images[index + 1];
            }
            OnPropertyChanged(nameof(CurrentIndex));
        }
        private bool CanMovePrevious()
        {
            return Images != null && Images.Count > 0 && CurrentIndex > 1;
        }
        private bool CanMoveNext()
        {
            return Images != null && Images.Count > 0 && CurrentIndex < Images.Count ;
        }

        [RelayCommand]
        private void ShowImage()
        {
            int height = (CurrentImage.Height / 9525) * 2;
            int width = (CurrentImage.Width / 9525) * 2;

            

            PopupHeight = height;
            PopupWidth = width;
            ShowImagePopup = true;
            
        }

        public void AddImage (PraccingImage image)
        {
            Images.Add(image);
            OnPropertyChanged(nameof(Images));
            CurrentImage = image;
            OnPropertyChanged(nameof(TotalImages));
            OnPropertyChanged(nameof(CurrentIndex));
        }

        public void RemoveImage(PraccingImage image)
        {
            Images.Remove(image);
            OnPropertyChanged(nameof(Images));
            OnPropertyChanged(nameof(TotalImages));
            OnPropertyChanged(nameof(CurrentIndex));
            CurrentImage = Images?.FirstOrDefault(); ;
        }
    }
}
