using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;

namespace SDIFrontEnd_WPF
{
    public interface IApiAuditService
    {
        Task<List<string>> GetAuditSurveys();
        Task<List<VariableName>> GetAuditVarNames(string survey);

        Task<List<ChangedSurveyQuestion>> GetQuestionHistory(int qid);

        Task<List<AuditWording>> GetWordingHistory(string wordType, int wordID);


    }
}
