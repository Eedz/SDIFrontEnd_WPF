using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using MvvmLib.ViewModels;
using ITCLib;

namespace SDIFrontEnd_WPF.ViewModels
{
    public class CommentsViewModel : ViewModelBase
    {
        public ObservableCollection<QuestionComment> Comments { get; set; }
        public ObservableCollection<Person> People { get; set; }
        public ObservableCollection<CommentType> Types { get; set; }

        public CommentsViewModel()
        {
            Comments = new ObservableCollection<QuestionComment>();
            People = new ObservableCollection<Person>();
            Types = new ObservableCollection<CommentType>();
        }

        public CommentsViewModel(ObservableCollection<QuestionComment> comments, ObservableCollection<Person> authors, ObservableCollection<CommentType> types) 
        {
            Comments = comments;
            People = authors;
            Types = types;

        }
    }
}
