using ITCLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    public class ReferenceDataService
    {
        private readonly IApiReferenceDataService _api;
        private readonly ReferenceDataStore _store;

        public ReferenceDataService(
            IApiReferenceDataService api,
            ReferenceDataStore store)
        {
            _api = api;
            _store = store;
        }

        public async Task LoadAsync()
        {
            var data = await _api.GetReferenceData();
          
            Replace(_store.DomainLabels, data.DomainLabels);
            Replace(_store.TopicLabels, data.TopicLabels);
            Replace(_store.ContentLabels, data.ContentLabels);
            Replace(_store.ProductLabels, data.ProductLabels);


        }

        private void Replace<T>(
            ObservableCollection<T> target,
            IEnumerable<T> source)
        {
            target.Clear();
            foreach (var item in source)
                target.Add(item);
        }
    }
}
