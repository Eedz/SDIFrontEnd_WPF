using System;
using System.Collections;
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

namespace SDIFrontEnd_WPF.Views
{
    /// <summary>
    /// Interaction logic for PraccingImportItemView.xaml
    /// </summary>
    public partial class PraccingImportItemView : UserControl
    {
        public PraccingImportItemView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CategoryListProperty =
            DependencyProperty.Register(
                nameof(CategoryList),
                typeof(IEnumerable),
                typeof(PraccingImportItemView),
                new PropertyMetadata(null, CategoryListChanged));

        public IEnumerable CategoryList
        {
            get => (IEnumerable)GetValue(CategoryListProperty);
            set => SetValue(CategoryListProperty, value);
        }

        public static readonly DependencyProperty PeopleListProperty =
            DependencyProperty.Register(
                nameof(PeopleList),
                typeof(IEnumerable),
                typeof(PraccingImportItemView),
                new PropertyMetadata(null, PeopleListChanged));

        public IEnumerable PeopleList
        {
            get => (IEnumerable)GetValue(PeopleListProperty);
            set => SetValue(PeopleListProperty, value);
        }

        private static void CategoryListChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (PraccingImportItemView)d;

            // Put breakpoint here
        }

        private static void PeopleListChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (PraccingImportItemView)d;

            // Put breakpoint here
        }

        public static readonly DependencyProperty MoveIssueCommandProperty =
            DependencyProperty.Register(
                nameof(MoveIssueCommand),
                typeof(ICommand),
                typeof(PraccingImportItemView));

        public ICommand MoveIssueCommand
        {
            get => (ICommand)GetValue(MoveIssueCommandProperty);
            set => SetValue(MoveIssueCommandProperty, value);
        }

        public static readonly DependencyProperty MoveIssueCommandParameterProperty =
            DependencyProperty.Register(
                nameof(MoveIssueCommandParameter),
                typeof(object),
                typeof(PraccingImportItemView));

        public object MoveIssueCommandParameter
        {
            get => GetValue(MoveIssueCommandParameterProperty);
            set => SetValue(MoveIssueCommandParameterProperty, value);
        }
    }
}
