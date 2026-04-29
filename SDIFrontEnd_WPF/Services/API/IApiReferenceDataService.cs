using ITC_Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    public interface IApiReferenceDataService
    {
        Task<ReferenceDataStore> GetReferenceData();

    }
}
