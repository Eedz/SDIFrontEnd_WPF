using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SDIFrontEnd_WPF
{
    public static class FocusBehavior
    {
        public static readonly DependencyProperty SetSelectedItemOnFocusProperty =
            DependencyProperty.RegisterAttached(
                "SetSelectedItemOnFocus",
                typeof(bool),
                typeof(FocusBehavior),
                new PropertyMetadata(false, OnSetSelectedItemOnFocusChanged));

        public static bool GetSetSelectedItemOnFocus(DependencyObject obj) =>
            (bool)obj.GetValue(SetSelectedItemOnFocusProperty);

        public static void SetSetSelectedItemOnFocus(DependencyObject obj, bool value) =>
            obj.SetValue(SetSelectedItemOnFocusProperty, value);

        private static void OnSetSelectedItemOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if ((bool)e.NewValue)
                    element.GotFocus += Element_GotFocus;
                else
                    element.GotFocus -= Element_GotFocus;
            }
        }

        private static void Element_GotFocus(object sender, RoutedEventArgs e)
        {
            var control = sender as DependencyObject;
            var listBoxItem = FindParent<ListBoxItem>(control);

            if (listBoxItem != null)
            {
                var listBox = ItemsControl.ItemsControlFromItemContainer(listBoxItem) as ListBox;
                if (listBox != null && listBoxItem.DataContext != null)
                {
                    listBox.SelectedItem = listBoxItem.DataContext;
                }
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent;

                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }
    }
}