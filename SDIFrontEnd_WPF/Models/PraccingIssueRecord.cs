
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using ITCLib;
using SDIFrontEnd_WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    public partial class PraccingIssueRecord : ObservableObject, IRecord<PraccingIssue>, IDisposable
    {

        private bool newRecord;
        public bool NewRecord
        {
            get => newRecord;
            set => SetProperty(ref newRecord, value);
        }

        private bool dirty;
        public bool Dirty
        {
            get { return dirty; }
            set { SetProperty(ref dirty, value); OnPropertyChanged(nameof(ShouldSave)); }
        }

        [ObservableProperty]
        private bool deleted; // marked for deletion

        public bool ShouldSave
        {
            get =>
                Dirty ||
                AddedResponses.Count > 0 ||
                EditedResponses.Count > 0 ||
                DeletedResponses.Count > 0 ||
                AddedImages.Count > 0 ||
                DeletedImages.Count > 0 ||
                AddedResponseImages.Count > 0 ||
                DeletedResponseImages.Count > 0;

        }

        public PraccingIssue Item { get; set; }

        public ObservableCollection<PraccingResponse> AddedResponses { get; set; }
        public ObservableCollection<PraccingResponse> EditedResponses { get; set; }
        public ObservableCollection<PraccingResponse> DeletedResponses { get; set; }

        public ObservableCollection<PraccingImage> AddedImages { get; set; }
        public ObservableCollection<PraccingImage> DeletedImages { get; set; }

        public ObservableCollection<PraccingImage> AddedResponseImages { get; set; }
        public ObservableCollection<PraccingImage> DeletedResponseImages { get; set; }

        public PraccingIssueRecord()
        {
            Item = new PraccingIssue();
            AddedResponses = new ObservableCollection<PraccingResponse>();
            EditedResponses = new ObservableCollection<PraccingResponse>();
            DeletedResponses = new ObservableCollection<PraccingResponse>();

            AddedImages = new ObservableCollection<PraccingImage>();
            DeletedImages = new ObservableCollection<PraccingImage>();

            AddedResponseImages = new ObservableCollection<PraccingImage>();
            DeletedResponseImages = new ObservableCollection<PraccingImage>();
        }

        public PraccingIssueRecord(PraccingIssue item) : this()
        {
            Item = item;
            Item.PropertyChanged += Item_PropertyChanged;

            foreach (PraccingResponse r in Item.Responses)
            {
                r.PropertyChanged += ResponseEdited;
            }

            WatchCollection(AddedResponses);
            WatchCollection(EditedResponses);
            WatchCollection(DeletedResponses);
            WatchCollection(AddedImages);
            WatchCollection(DeletedImages);
            WatchCollection(AddedResponseImages);
            WatchCollection(DeletedResponseImages);
        }


        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var property = e.PropertyName;
            Dirty = true;
        }

        private void ResponseEdited(object sender, PropertyChangedEventArgs e)
        {
            if (sender is PraccingResponse r && !EditedResponses.Contains(r) && !AddedResponses.Contains(r))
            {
                EditedResponses.Add(r);
            }
        }

        private void WatchCollection(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += (_, __) => OnPropertyChanged(nameof(ShouldSave));
        }



        public void AddResponse(PraccingResponse response)
        {
            if (response == null) return;
            if (AddedResponses.Contains(response) || EditedResponses.Contains(response)) return;
            AddedResponses.Add(response);
            response.PropertyChanged += ResponseEdited;
            Item.Responses.Add(response);
        }

        public void Dispose()
        {
            Item.PropertyChanged -= Item_PropertyChanged;
            foreach (PraccingResponse r in Item.Responses)
            {
                r.PropertyChanged -= ResponseEdited;
            }
            //AddedImages.CollectionChanged -= (_, __) => OnPropertyChanged(nameof(ShouldSave));
        }
    }
}
