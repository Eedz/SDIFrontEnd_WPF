using ITC_Services;
using ITCLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF
{
    
    public class LookupProvider
    {
        public IReadOnlyList<SurveyMode> Modes { get; }
        public IReadOnlyList<UserState> UserStates { get; }
        public IReadOnlyList<Language> Languages { get; }
        public IReadOnlyList<ScreenedProduct> ScreenedProducts { get; }

        public IReadOnlyList<SurveyCohort> Cohorts { get; }

        public LookupProvider(IReferenceDataService _dataService)
        {
            Cohorts = _dataService.GetCohortInfo();
            Modes = _dataService.GetModeInfo();
            UserStates = _dataService.GetUserStates();
            Languages = _dataService.GetLanguages();
            ScreenedProducts = _dataService.GetScreenedProducts();

        }
    }
    
}
