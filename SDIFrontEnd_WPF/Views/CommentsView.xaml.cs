using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using ITCLib;


namespace SurveyImporter_WPF.Views
{
    /// <summary>
    /// Interaction logic for CommentsView.xaml
    /// </summary>
    public partial class CommentsView : UserControl
    {
        //public static readonly DependencyProperty CommentsProperty =
        //    DependencyProperty.Register("Comments", typeof(ObservableCollection<QuestionComment>), typeof(CommentsView), new PropertyMetadata(null));

        public static readonly DependencyProperty PeopleProperty =
            DependencyProperty.Register("People", typeof(ObservableCollection<Person>), typeof(CommentsView), new PropertyMetadata(null));

        public static readonly DependencyProperty TypesProperty =
            DependencyProperty.Register("Types", typeof(ObservableCollection<CommentType>), typeof(CommentsView), new PropertyMetadata(null));

        //public ObservableCollection<QuestionComment> Comments
        //{
        //    get => (ObservableCollection<QuestionComment>)GetValue(CommentsProperty);
        //    set => SetValue(CommentsProperty, value);
        //}

        public ObservableCollection<Person> People
        {
            get => (ObservableCollection<Person>)GetValue(PeopleProperty);
            set => SetValue(PeopleProperty, value);
        }

        public ObservableCollection<CommentType> Types
        {
            get => (ObservableCollection<CommentType>)GetValue(TypesProperty);
            set => SetValue(TypesProperty, value);
        }

        public CommentsView()
        {
            InitializeComponent();
        }
    }
}
