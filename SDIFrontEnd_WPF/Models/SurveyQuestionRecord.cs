using CommunityToolkit.Mvvm.ComponentModel;
using ITCLib;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
namespace SDIFrontEnd_WPF
{
    public partial class SurveyQuestionRecord : ObservableObject, IRecord<SurveyQuestion>, IDisposable
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(QuestionStatus))]
        private bool newRecord;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(QuestionStatus))]
        [NotifyPropertyChangedFor(nameof(ShouldSave))]
        private bool dirty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(QuestionStatus))]
        [NotifyPropertyChangedFor(nameof(ShouldSave))]
        private bool dirtyWordings;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(QuestionStatus))]
        [NotifyPropertyChangedFor(nameof(ShouldSave))]
        private bool deleted; // marked for deletion

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(QuestionStatus))]
        [NotifyPropertyChangedFor(nameof(ShouldSave))]
        private bool dirtyQnum;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(QuestionStatus))]
        [NotifyPropertyChangedFor(nameof(ShouldSave))]
        private bool dirtyLabels;

        public ObservableCollection<QuestionTimeFrame> AddTimeFrames;
        public ObservableCollection<QuestionTimeFrame> DeleteTimeFrames;

        public ObservableCollection<SurveyImage> AddedImages;
        public ObservableCollection<SurveyImage> DeletedImages;       

        public SurveyQuestion Item { get; set; }

        public string QuestionStatus
        {
            get
            {
                if (NewRecord) return "new";
                if (Deleted) return "deleted";
                if (ShouldSave) return "edited";
                return "unchanged";
            }
        }



        public bool ShouldSave
        {
            get =>
                DirtyWordings ||
                DirtyQnum ||
                DirtyLabels ||
                (AddTimeFrames != null && AddTimeFrames.Count > 0) ||
                (DeleteTimeFrames != null && DeleteTimeFrames.Count > 0) ||
                (AddedImages != null && AddedImages.Count > 0) ||
                (DeletedImages != null && DeletedImages.Count > 0);
        }

        public SurveyQuestionRecord(SurveyQuestion item)
        {
            Item = item;
            if (item is INotifyPropertyChanged npc)
            {
                npc.PropertyChanged += Item_PropertyChanged;
                item.VarName.PropertyChanged += Item_PropertyChanged;
            }

            AddTimeFrames = new ObservableCollection<QuestionTimeFrame>();
            DeleteTimeFrames = new ObservableCollection<QuestionTimeFrame>();

            AddedImages = new ObservableCollection<SurveyImage>();
            DeletedImages = new ObservableCollection<SurveyImage>();

            WatchCollection(AddTimeFrames);
            WatchCollection(DeleteTimeFrames);
            WatchCollection(AddedImages);
            WatchCollection(DeletedImages);
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SurveyQuestion.QuestionType))
                return;
            // ignore selection changes

            if (e.PropertyName == nameof(SurveyQuestion.PrePW) ||
                e.PropertyName == nameof(SurveyQuestion.PreIW) ||
                e.PropertyName == nameof(SurveyQuestion.PreAW) ||
                e.PropertyName == nameof(SurveyQuestion.LitQW) ||
                e.PropertyName == nameof(SurveyQuestion.PstIW) ||
                e.PropertyName == nameof(SurveyQuestion.PstPW) ||
                e.PropertyName == nameof(SurveyQuestion.RespOptionsS) ||
                e.PropertyName == nameof(SurveyQuestion.NRCodesS)
                )
                DirtyWordings = true;

            if (e.PropertyName == nameof(SurveyQuestion.Qnum))
                DirtyQnum = true;
            if (e.PropertyName == nameof(SurveyQuestion.VarName.Topic) ||
                e.PropertyName == nameof(SurveyQuestion.VarName.Content) ||
                e.PropertyName == nameof(SurveyQuestion.VarName.Product) ||
                e.PropertyName == nameof(SurveyQuestion.VarName.VarLabel) || 
                e.PropertyName == nameof(SurveyQuestion.VarName.TopicLabel) ||
                e.PropertyName == nameof(SurveyQuestion.VarName.ContentLabel) ||
                e.PropertyName == nameof(SurveyQuestion.VarName.ProductLabel) 
                )
                DirtyLabels = true;

            OnPropertyChanged(nameof(Dirty));
            OnPropertyChanged(nameof(DirtyQnum));
            OnPropertyChanged(nameof(DirtyLabels));
            OnPropertyChanged(nameof(QuestionStatus));
        }

        private void WatchCollection(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += (sender, e) => OnCollectionChanged(e);
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ShouldSave));
            OnPropertyChanged(nameof(QuestionStatus));
        }

        public void Dispose()
        {
            if (Item is INotifyPropertyChanged npc)
            {
                npc.PropertyChanged -= Item_PropertyChanged;
                Item.VarName.PropertyChanged -= Item_PropertyChanged;
            }
        }
    }
}
