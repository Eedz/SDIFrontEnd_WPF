using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace SDIFrontEnd_WPF
{
    public class HtmlToFlowDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                string xaml = HtmlToXaml.HtmlToXamlConverter.ConvertHtmlToXaml((string)value, true);
                FlowDocument flow = (FlowDocument) XamlReader.Parse(xaml);
                flow.TextAlignment = System.Windows.TextAlignment.Left;
                flow.FontFamily = new System.Windows.Media.FontFamily("Verdana");
                flow.FontSize = 12;
                return flow;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

