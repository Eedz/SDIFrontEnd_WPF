using ITCLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    public class WordingDataService
    {
        private readonly IApiWordingService _api;
        private readonly WordingData _store;

        public WordingDataService(IApiWordingService api, WordingData store)
        {
            _api = api;
            _store = store;
        }

        public async Task LoadAsync()
        {
            var prep = await _api.GetAllPreP();
            var prei = await _api.GetAllPreI();
            var prea = await _api.GetAllPreA();
            var litq = await _api.GetAllLitQ();
            var psti = await _api.GetAllPstI();
            var pstp = await _api.GetAllPstP();
            var ro = await _api.GetAllRespOptions();
            var nr = await _api.GetAllNonResponses();

            Replace(_store.PreP, prep);
            Replace(_store.PreI, prei);
            Replace(_store.PreA, prea);
            Replace(_store.LitQ, litq);
            Replace(_store.PstI, psti);
            Replace(_store.PstP, pstp);
            Replace(_store.RO, ro);
            Replace(_store.NR, nr);
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

    public class WordingData
    {
        public ObservableCollection<Wording> PreP { get; set; } = new();
        public ObservableCollection<Wording> PreI { get; set; } = new();
        public ObservableCollection<Wording> PreA { get; set; } = new();
        public ObservableCollection<Wording> LitQ { get; set; } = new();
        public ObservableCollection<Wording> PstI { get; set; } = new();
        public ObservableCollection<Wording> PstP { get; set;  } = new();
        public ObservableCollection<ResponseSet> RO { get; set; } = new();
        public ObservableCollection<ResponseSet> NR { get; set; } = new();
    }
}
