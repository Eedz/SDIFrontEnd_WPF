using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;
namespace SDIFrontEnd_WPF.Models
{
	public class CurrentUserService : ICurrentUserService
	{
		private readonly IApiUserService _userService;
		public UserPrefs CurrentUser { get; set; } = new UserPrefs();

		public ObservableCollection<FormState> SurveyHistory { get; set; }

		public CurrentUserService(IApiUserService userService)
		{
			_userService = userService;
			SurveyHistory = new ObservableCollection<FormState>();
			_ = GetUser();
		}

		public async Task GetUser()
		{
			CurrentUser = await _userService.GetUser(Environment.UserName) ?? throw new ArgumentNullException(nameof(_userService), "User preferences cannot be null");
			SurveyHistory = new ObservableCollection<FormState>(CurrentUser.FormStates ?? new List<FormState>());
		}

		public void UpdateFormState(FormState formState)
		{
			_userService.UpdateSurveyHistory(formState);
			SurveyHistory.Where(x=> x.PersonnelID == formState.PersonnelID && x.FormNum == formState.FormNum && x.FormName == formState.FormName).ToList().ForEach(x =>
			{
				x.FilterID = formState.FilterID;
			});
		}
	}
    
}
