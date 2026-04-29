using ITCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SDIFrontEnd_WPF
{
    public  interface IApiQuestionService
    {
        Task<SurveyQuestion?> GetQuestionByIdAsync(int id);
        Task<List<SurveyQuestion>> GetQuestionsByVarNameAsync(string varName);
        Task<List<SurveyQuestion>> GetQuestionsByRefVarNameAsync(string refVarName);

        Task<SurveyQuestionRecord> SaveQuestion(SurveyQuestionRecord question);

        Task<List<SurveyQuestion>> SearchQuestions(string searchTerm);

        Task<int> AddQuestion(SurveyQuestion question);
        Task<int> UpdateQuestion(SurveyQuestion question);
        Task<int> DeleteQuestion(SurveyQuestion question);
    }
}
