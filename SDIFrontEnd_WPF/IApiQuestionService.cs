using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;
namespace SDIFrontEnd_WPF
{
    public  interface IApiQuestionService
    {
        Task<SurveyQuestion?> GetQuestionByIdAsync(int id);
        Task<List<SurveyQuestion>> GetQuestionsByVarNameAsync(string varName);
        Task<List<SurveyQuestion>> GetQuestionsByRefVarNameAsync(string refVarName);

        Task<SurveyQuestion> SaveQuestion(SurveyQuestion question);
    }
}
