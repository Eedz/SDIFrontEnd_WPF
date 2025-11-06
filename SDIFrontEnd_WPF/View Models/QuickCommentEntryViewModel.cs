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

        private SurveyQuestion Question;

        public QuestionComment NewComment;

        public QuickCommentEntryViewModel(IPeopleService peopleService, ICommentService commentService, SurveyQuestion question)
        {
            _peopleService = peopleService ?? throw new ArgumentNullException(nameof(peopleService));
            _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));

            base.DisplayName = "Comment Entry";
            Question = question;
            _ = LoadLists();
        }

        private async Task LoadLists()
        {
            AllAuthors = await _peopleService.GetPeopleBasicsAsync();
            AllCommentTypes = _commentService.GetAllCommentTypes();
        }


        [RelayCommand (CanExecute ="CanSave")]
        private void SaveComment()
        {
            NewComment = new QuestionComment
            {
                Survey = Question.SurveyCode,
                VarName = Question.VarName.VarName,
                Notes = new Note() { NoteText = this.NoteText },
                Author = new Person(Author.FirstName, Author.ID),
                NoteDate = this.NoteDate,
                NoteType = this.NoteType,
                Source = this.NoteSource,
                Authority = new Person(Authority.FirstName, Authority.ID)
            };

            if (_commentService.InsertQuestionComment(NewComment) == 0)
                throw new Exception("Error saving comment.");

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
