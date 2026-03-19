using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ITCLib;

namespace SDIFrontEnd_WPF
{
    public class ApiAuditService : ApiServiceBase, IApiAuditService
    {
        public ApiAuditService(HttpClient http) : base(http)
        {
        }

        public async Task<List<string>> GetAuditSurveys()
        {
            throw new NotImplementedException();
        }

        public async Task<List<VariableName>> GetAuditVarNames(string survey)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ChangedSurveyQuestion>> GetQuestionHistory(int qid)
        {
            throw new NotImplementedException();
        }

        public async Task<List<AuditWording>> GetWordingHistory(string wordType, int wordID)
        {
            throw new NotImplementedException();
        }

   
    }
}
