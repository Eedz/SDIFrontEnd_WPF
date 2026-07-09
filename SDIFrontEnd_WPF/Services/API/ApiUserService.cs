using ITCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using ITC_Contracts;
namespace SDIFrontEnd_WPF
{
    public class ApiUserService : ApiServiceBase, IApiUserService
    {
        public ApiUserService(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<UserPrefs> GetUser(string username)
        {
            try
            {
                var response = await this._http.GetFromJsonAsync<UserPrefsDto>($"{_baseApi}/users?username={username}");
                if (response == null)
                    return new UserPrefs();

                var userPrefs = new UserPrefs
                {
                    userid = response.userid,
                    Username = response.Username,
                    accessLevel = (AccessLevel)response.accessLevel,
                    ReportPath = response.ReportPath,
                    reportPrompt = response.reportPrompt,
                    wordingNumbers = response.wordingNumbers,
                    commentDetails = response.commentDetails,
                    //LastUsedComment = response.LastUsedComment,
                    //SavedComments = response.SavedComments,
                    //SavedSources = response.SavedSources,
                    FormStates = response.FormStates.Select(x=> new FormState()
                    {
                        ID = x.ID,
                        Filter = x.Filter,
                        FilterID = x.FilterID,
                        PersonnelID = x.PersonnelID,
                        FormName = x.FormName,
                        FormNum = x.FormNum,
                        RecordPosition = x.RecordPosition,
                    }).ToList()
                };
                return userPrefs;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new ApplicationException("Error fetching user preferences", ex);
            }
        }
    }
}
