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

namespace SDIFrontEnd_WPF
{
    /// <summary>
    /// Interaction logic for NavigationBar.xaml
    /// </summary>
    public partial class NavigationBar : UserControl
    {
        public NavigationBar()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty FirstItemCommandProperty =
            DependencyProperty.Register(nameof(FirstItemCommand), typeof(ICommand), typeof(NavigationBar));

        public static readonly DependencyProperty PreviousItemCommandProperty =
            DependencyProperty.Register(nameof(PreviousItemCommand), typeof(ICommand), typeof(NavigationBar));

        public static readonly DependencyProperty NextItemCommandProperty =
            DependencyProperty.Register(nameof(NextItemCommand), typeof(ICommand), typeof(NavigationBar));

        public static readonly DependencyProperty LastItemCommandProperty =
            DependencyProperty.Register(nameof(LastItemCommand), typeof(ICommand), typeof(NavigationBar));

        public static readonly DependencyProperty ItemPositionProperty =
            DependencyProperty.Register(nameof(ItemPosition), typeof(string), typeof(NavigationBar));

        public ICommand FirstItemCommand
        {
            get => (ICommand)GetValue(FirstItemCommandProperty);
            set => SetValue(FirstItemCommandProperty, value);
        }

        public ICommand PreviousItemCommand
        {
            get => (ICommand)GetValue(PreviousItemCommandProperty);
            set => SetValue(PreviousItemCommandProperty, value);
        }

        public ICommand NextItemCommand
        {
            get => (ICommand)GetValue(NextItemCommandProperty);
            set => SetValue(NextItemCommandProperty, value);
        }

        public ICommand LastItemCommand
        {
            get => (ICommand)GetValue(LastItemCommandProperty);
            set => SetValue(LastItemCommandProperty, value);
        }

        public string ItemPosition
        {
            get => (string)GetValue(ItemPositionProperty);
            set => SetValue(ItemPositionProperty, value);
        }
    }
}
