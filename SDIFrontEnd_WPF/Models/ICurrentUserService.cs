using ITCLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF.Models
{
    public interface ICurrentUserService
    {
        
		UserPrefs CurrentUser {get;set; }
		public ObservableCollection<FormState> SurveyHistory { get; set; }

		void UpdateFormState(FormState formState);
	}


}
