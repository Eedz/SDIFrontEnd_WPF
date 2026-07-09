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


        Task<List<SurveyQuestion>> SearchQuestions(string searchTerm);

        Task<int> AddQuestion(SurveyQuestion question);
        Task<int> UpdateQuestion(SurveyQuestion question);
        Task<bool> DeleteQuestion(SurveyQuestion question);
        Task<bool> UpdateQnums(IEnumerable<SurveyQuestion> qnums);

        Task<bool> UpdateTranslation(Translation translation);
        Task<bool> CreateTranslation(Translation translation);
    }
}
