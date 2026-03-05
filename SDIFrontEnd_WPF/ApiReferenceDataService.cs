using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using ITC_Contracts;
using ITCLib;
using System.Collections.ObjectModel;
namespace SDIFrontEnd_WPF
{
    public class ApiReferenceDataService : ApiServiceBase, IApiReferenceDataService
    {
       

        public ApiReferenceDataService(HttpClient http) : base(http)
        {

        }

        public async Task<ReferenceDataStore> GetReferenceData()
        {
            var dto = await _http.GetFromJsonAsync<ReferenceDataDto>("api/reference-data");
            if (dto == null) return new ReferenceDataStore();
            var referenceData = new ReferenceDataStore()
            {
                DomainLabels = new ObservableCollection<VarNameLabel>(dto.DomainLabels.Select(x => new VarNameLabel() { ID = x.ID, Label = x.LabelText })),
                ContentLabels = new ObservableCollection<VarNameLabel>(dto.ContentLabels.Select(x => new VarNameLabel() { ID = x.ID, Label = x.LabelText })),
                TopicLabels = new ObservableCollection<VarNameLabel>(dto.TopicLabels.Select(x => new VarNameLabel() { ID = x.ID, Label = x.LabelText })),
                ProductLabels = new ObservableCollection<VarNameLabel>(dto.ProductLabels.Select(x => new VarNameLabel() { ID = x.ID, Label = x.LabelText }))

            };
                
            
            return referenceData;
        }
    }
}
