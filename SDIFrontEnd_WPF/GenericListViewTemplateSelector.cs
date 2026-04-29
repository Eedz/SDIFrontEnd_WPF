using SDIFrontEnd_WPF.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SDIFrontEnd_WPF
{
    public class GenericListViewTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
                return null;

            var itemType = item.GetType();

            // We only handle ListViewModel<T>
            if (itemType.IsGenericType && itemType.GetGenericTypeDefinition() == typeof(ListViewModel<>))
            {
                var elementType = itemType.GetGenericArguments()[0];
                var viewName = $"{elementType.Name}ListView";

                // Use the same assembly as the ViewModel
                var assembly = itemType.Assembly;

                // Try to find a type ending with {TypeName}ListView
                var viewType = assembly
                    .GetTypes()
                    .FirstOrDefault(t =>
                        t.Name.Equals(viewName, StringComparison.OrdinalIgnoreCase) &&
                        typeof(FrameworkElement).IsAssignableFrom(t));

                if (viewType != null)
                {
                    var factory = new FrameworkElementFactory(viewType);
                    return new DataTemplate { VisualTree = factory };
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}
