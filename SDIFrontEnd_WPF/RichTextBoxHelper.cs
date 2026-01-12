
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

namespace SDIFrontEnd_WPF
{
    public static class RichTextBoxHelper
    {
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.RegisterAttached("Document", typeof(FlowDocument), typeof(RichTextBoxHelper),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDocumentChanged));

        public static string ConvertFlowDocumentToXaml(FlowDocument doc)
        {
            if (doc == null) return string.Empty;

            return XamlWriter.Save(doc);
        }

        public static void SetDocument(DependencyObject obj, FlowDocument value)
        {
            obj.SetValue(DocumentProperty, value);
        }

        public static FlowDocument GetDocument(DependencyObject obj)
        {
            return (FlowDocument)obj.GetValue(DocumentProperty);
        }

        private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichTextBox richTextBox && e.NewValue !=null)
            {
                richTextBox.Document = e.NewValue as FlowDocument;
                richTextBox.TextChanged += (s, _) =>
                {
                    var rtb = s as RichTextBox;
                    SetDocument(rtb, rtb.Document);
                };
            }
        }
    }
}
