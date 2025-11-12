using CommunityToolkit.Mvvm.Input;
using ITC_Services;
using ITCLib;

using MvvmLib.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class QuickCommentEntryViewModel : WorkspaceViewModel
    {
        private readonly IPeopleService _peopleService;
        private readonly ICommentService _commentService;
        public List<Person> AllAuthors { get; set; }
        public  List<CommentType> AllCommentTypes { get; set; }
        public string NoteText { get; set; } = string.Empty;
        public Person Author { get; set; } = new Person();
        public CommentType NoteType { get; set; } = new CommentType();
        public DateTime NoteDate { get; set; } = DateTime.Now;
        public string NoteSource { get; set; } = string.Empty;
        public Person Authority { get; set; } = new Person();

        public Comment NewComment;

        public QuickCommentEntryViewModel(IPeopleService peopleService, ICommentService commentService)
        {
            _peopleService = peopleService ?? throw new ArgumentNullException(nameof(peopleService));
            _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));

            base.DisplayName = "Comment Entry";
            _ = LoadLists();
        }

        private async Task LoadLists()
        {
            AllAuthors = await _peopleService.GetPeopleBasicsAsync();
            AllCommentTypes = _commentService.GetAllCommentTypes();
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
            return true;
            return !string.IsNullOrWhiteSpace(NoteText);
        }
    }
}
