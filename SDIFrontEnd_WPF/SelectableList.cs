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
        private bool isSelected;

        public T? Value { get; }
        public bool IsAll { get; }

        public SelectableItem(T value)
        {
            Value = value;
            IsAll = false;
        }

        public SelectableItem(bool isAll)
        {
            IsAll = isAll;
        }
        public override string ToString()
        {
            return IsAll ? "<All>" : Value?.ToString() ?? "";
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
                AllItem = new SelectableItem<T>(isAll: true);
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

            var changed = (SelectableItem<T>)sender!;

            _suppress = true;

            if (AllItem != null)
            {
                if (changed.IsAll && AllItem.IsSelected)
                {
                    // <All> selected -> unselect all others
                    foreach (var item in Items)
                    {
                        if (!item.IsAll)
                            item.IsSelected = false;
                    }
                }
                else if (!changed.IsAll && changed.IsSelected)
                {
                    // A normal item selected -> deselect <All>
                    AllItem.IsSelected = false;
                }
            }

            _suppress = false;
        }

        public IEnumerable<T> SelectedValues =>
            Items.Where(i => i.IsSelected && !i.IsAll).Select(i => i.Value!);

        public bool IsAllSelected => AllItem?.IsSelected == true;
    }
}
