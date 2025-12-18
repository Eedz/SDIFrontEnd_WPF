using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;

namespace SDIFrontEnd_WPF
{
    public interface IMatrixService
    {
        Task<IList<SurveyQuestion>> LoadSurveyQuestionsAsync(int surveyId);

        IList<SurveyQuestion> MergeQuestions(IEnumerable<Survey> surveys);

        IList<SurveyQuestion> SortQuestions(IEnumerable<SurveyQuestion> questions);
        IList<SurveyQuestion> SortQuestionsQnum(IEnumerable<SurveyQuestion> questions);

        IList<IReadOnlyList<string>> BuildMatrix(
            IList<SurveyQuestion> questions,
            IList<Survey> surveys);
    }
}
