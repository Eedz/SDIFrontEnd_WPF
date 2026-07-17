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
    /// Interaction logic for PraccingImportResponseItemView.xaml
    /// </summary>
    public partial class PraccingImportResponseItemView : UserControl
    {
        public PraccingImportResponseItemView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty NewResponseProperty =
            DependencyProperty.Register(
                nameof(NewResponse),
                typeof(bool), 
                typeof(PraccingImportResponseItemView));

        public bool NewResponse
        {
            get => (bool)GetValue(NewResponseProperty);
            set => SetValue(NewResponseProperty, value);
        }
    }
}
