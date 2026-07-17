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

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class QuickCommentEntryViewModel : WorkspaceViewModel
    {
        private readonly IApiPeopleService _peopleService;
        private readonly ReferenceDataStore _commentData;
        public ObservableCollection<Person> AllAuthors { get; set; }
        public ObservableCollection<CommentType> AllCommentTypes { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommentCommand))]
        private string noteText = string.Empty;
        public Person Author { get; set; } = new Person();
        public CommentType NoteType { get; set; } = new CommentType();
        public DateTime NoteDate { get; set; } = DateTime.Now;
        public string NoteSource { get; set; } = string.Empty;
        public Person Authority { get; set; } = new Person();

        public Comment NewComment;

        public QuickCommentEntryViewModel(IApiPeopleService peopleService, ReferenceDataStore commentService)
        {
            _peopleService = peopleService ?? throw new ArgumentNullException(nameof(peopleService));
            _commentData = commentService ?? throw new ArgumentNullException(nameof(commentService));

            base.DisplayName = "Comment Entry";
        }

        public async Task LoadLists()
        {
            AllAuthors = new ObservableCollection<Person>(await _peopleService.GetPeopleBasics());
            AllCommentTypes = new ObservableCollection<CommentType>(_commentData.CommentTypes.ToList());
            OnPropertyChanged(nameof(AllAuthors));
            OnPropertyChanged(nameof(AllCommentTypes));
        }


        [RelayCommand (CanExecute ="CanSave")]
        private void SaveComment()
        {
            NewComment = new Comment
            {
                Notes = new Note() { NoteText = this.NoteText },
                Author = new Person(Author.FirstName, Author.ID),
                NoteDate = this.NoteDate,
                NoteType = this.NoteType,
                Source = this.NoteSource,
                Authority = new Person(Authority.FirstName, Authority.ID)
            };
            OnRequestClose(true);
        }

        [RelayCommand]
        private void Cancel()
        {
            OnRequestClose(false);
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(NoteText);
        }
    }
}
