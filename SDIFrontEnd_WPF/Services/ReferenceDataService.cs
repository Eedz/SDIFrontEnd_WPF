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
        private readonly IApiCommentService _apiComments;
        private readonly ReferenceDataStore _store;

        public ReferenceDataService(
            IApiReferenceDataService api, IApiCommentService commentService,
            ReferenceDataStore store)
        {
            _api = api;
            _apiComments = commentService;
            _store = store;
        }

        public async Task LoadAsync()
        {
            var data = await _api.GetReferenceData();
          
            Replace(_store.DomainLabels, data.DomainLabels);
            Replace(_store.TopicLabels, data.TopicLabels);
            Replace(_store.ContentLabels, data.ContentLabels);
            Replace(_store.ProductLabels, data.ProductLabels);
            
            var commentData = await _apiComments.GetCommentTypesAsync();
            Replace(_store.CommentTypes, commentData);

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
