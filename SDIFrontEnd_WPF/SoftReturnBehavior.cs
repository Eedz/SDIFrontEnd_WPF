using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;


namespace SDIFrontEnd_WPF.Behaviors
{
    public static class RichTextBoxSoftReturnBehavior
    {
        public static readonly DependencyProperty EnableSoftReturnProperty =
            DependencyProperty.RegisterAttached(
                "EnableSoftReturn",
                typeof(bool),
                typeof(RichTextBoxSoftReturnBehavior),
                new PropertyMetadata(false, OnEnableSoftReturnChanged));

        public static void SetEnableSoftReturn(DependencyObject element, bool value)
            => element.SetValue(EnableSoftReturnProperty, value);

        public static bool GetEnableSoftReturn(DependencyObject element)
            => (bool)element.GetValue(EnableSoftReturnProperty);

        private static void OnEnableSoftReturnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichTextBox rtb)
            {
                if ((bool)e.NewValue)
                    rtb.PreviewKeyDown += OnPreviewKeyDown;
                else
                    rtb.PreviewKeyDown -= OnPreviewKeyDown;
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            var rtb = (RichTextBox)sender;

            // OPTIONAL: allow Shift+Enter to behave normally if you want
            // if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            //     return;

            e.Handled = true;

            var caret = rtb.CaretPosition;

            //var lineBreak = new LineBreak();
            //caret.InsertInline(lineBreak);
            //rtb.CaretPosition = lineBreak.ElementEnd;

            caret.InsertLineBreak();
            
            var next = caret.GetNextInsertionPosition(LogicalDirection.Forward);
            if (next != null)
                rtb.CaretPosition = next;
        }
    }
}