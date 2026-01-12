using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SDIFrontEnd_WPF
{
    public partial class SelectableItem<T> : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected;

        public T Value { get; }
        

        public SelectableItem(T value)
        {
            Value = value;
        }

        public override string ToString()
        {
            // For display in ListBox
            return Value?.ToString() ?? "";
        }
    }

    public class SelectableList<T>
    {
        public ObservableCollection<SelectableItem<T>> Items { get; }
        = new ObservableCollection<SelectableItem<T>>();

        public SelectableItem<T>? AllItem { get; private set; }

        private bool _suppress;

        public SelectableList(IEnumerable<T> values, bool includeAll = true)
        {
            _suppress = true;

            if (includeAll)
            {
                AllItem = new SelectableItem<T>((T)(object)"<All>");
                Items.Add(AllItem);
            }

            foreach (var v in values)
                Items.Add(new SelectableItem<T>(v));

            _suppress = false;

            foreach (var item in Items)
                item.PropertyChanged += Item_PropertyChanged;
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SelectableItem<T>.IsSelected)) return;
            if (_suppress) return;

            var changed = (SelectableItem<T>)sender;

            _suppress = true;

            if (AllItem != null)
            {
                if (changed == AllItem && AllItem.IsSelected)
                {
                    // <All> selected → unselect all others
                    foreach (var item in Items)
                    {
                        if (item != AllItem)
                            item.IsSelected = false;
                    }
                }
                else if (changed != AllItem && changed.IsSelected)
                {
                    // A normal item selected → deselect <All>
                    AllItem.IsSelected = false;
                }
            }

            _suppress = false;
        }

        public IEnumerable<T> SelectedValues =>
            Items.Where(i => i.IsSelected && i != AllItem).Select(i => i.Value);

        public bool IsAllSelected => AllItem != null && AllItem.IsSelected;
    }
}
