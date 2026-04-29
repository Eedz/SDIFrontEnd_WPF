using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;

namespace SDIFrontEnd_WPF
{ 
    public class MatrixService : IMatrixService
    {
        private readonly IApiSurveyService _surveyService;

        public MatrixService(IApiSurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        public async Task<IList<SurveyQuestion>> LoadSurveyQuestionsAsync(int surveyId)
        {
            return await _surveyService.GetSurveyQuestions(surveyId);
        }

        public IList<SurveyQuestion> MergeQuestions(IEnumerable<Survey> surveys)
        {
            var list = new List<SurveyQuestion>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var survey in surveys)
            {
                foreach (var q in survey.Questions)
                {
                    string key = q.VarName?.RefVarName ?? "";

                    if (seen.Add(key))
                        list.Add(q);
                }
            }

            return list;
        }

        public IList<SurveyQuestion> SortQuestions(IEnumerable<SurveyQuestion> questions)
        {
            return questions
                .OrderBy(q => q.VarName?.RefVarName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public IList<SurveyQuestion> SortQuestionsQnum(IEnumerable<SurveyQuestion> questions)
        {
            return questions
                .OrderBy(q => q.Qnum, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public IList<IReadOnlyList<string>> BuildMatrix(
            IList<SurveyQuestion> questions,
            IList<Survey> surveys)
        {
            var result = new List<IReadOnlyList<string>>();

            foreach (var q in questions)
            {
                var row = new List<string>();

                foreach (var s in surveys)
                {
                    bool exists = s.Questions
                        .Any(x => x.VarName.RefVarName == q.VarName.RefVarName);

                    row.Add(exists ? q.Qnum : string.Empty);
                }

                result.Add(row);
            }

            return result;
        }
    }
}
