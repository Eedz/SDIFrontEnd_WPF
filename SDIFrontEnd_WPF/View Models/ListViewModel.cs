using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MvvmLib.ViewModels;

namespace SDIFrontEnd_WPF.ViewModels
{
    public partial class ListViewModel<T> : WorkspaceViewModel
    {
        private readonly IEnumerable<T> _items;
        public IEnumerable<T> Items => _items;

        [ObservableProperty]
        private T? selectedItem;
        
        public ListViewModel(IEnumerable<T> items, string displayName)
        {
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            _items = items ?? throw new ArgumentNullException(nameof(items));
        }
    }
}
