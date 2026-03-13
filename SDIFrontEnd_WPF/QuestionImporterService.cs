using DocumentFormat.OpenXml.Drawing;
using ITC_Services;
using ITCLib;
using ITCReportLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
namespace SDIFrontEnd_WPF
{
    public class QuestionImporterService
    {
        private readonly IApiSurveyService _surveyService;
        private readonly WordingData _wordingDataService;
        private readonly IPeopleService _peopleService;
        private readonly ICommentService _commentService;

        public QuestionImporterService(
            IApiSurveyService surveyService,
            IPeopleService peopleService,
            ICommentService commentService,
            WordingData wordingDataService)
        {
            _surveyService = surveyService;
            _peopleService = peopleService;
            _commentService = commentService;
            _wordingDataService = wordingDataService;
        }

        public async Task<List<QuestionCandidatePreview>> ImportQuestions(string surveyCode, string sourceFilePath)
        {
            var importer = new QuestionImporter(surveyCode)
            {
                RevisionsOnly = true
            };

            importer.Import(sourceFilePath);
          
          
            var questions = importer.ImportedPreviews.ToList();
            var existingQuestions = await GetExistingQuestions(surveyCode);

            var people = _peopleService.GetPeopleBasics();
            var commentTypes = _commentService.GetAllCommentTypes();

            

            foreach (var q in questions)
            {
                EnrichCandidate(q, questions, existingQuestions, people, commentTypes, sourceFilePath);
            }

            return questions;
        }

        public async Task<List<SurveyQuestion>> GetExistingQuestions(string surveyCode)
        {
            var survey = (await _surveyService.GetAllAsync()).FirstOrDefault(s => s.SurveyCode == surveyCode);
            if (survey == null)
                throw new ArgumentException($"Survey with code {surveyCode} not found.");
            return await _surveyService.GetSurveyQuestions(survey.SID);
        }

        public async Task<ObservableCollection<Wording>> GetMasterListFor(WordingType type)
        {
            return type switch
            {
                WordingType.PreP => _wordingDataService.PreP,
                WordingType.PreI => _wordingDataService.PreI,
                WordingType.PreA => _wordingDataService.PreA,
                WordingType.LitQ => _wordingDataService.LitQ,
                WordingType.PstI => _wordingDataService.PstI,
                WordingType.PstP => _wordingDataService.PstP,
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        public async Task<ObservableCollection<ResponseSet>> GetMasterListFor(ResponseType type)
        {
            return type switch
            {
                ResponseType.RespOptions => _wordingDataService.RO,
                ResponseType.NRCodes => _wordingDataService.NR,
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }


        /// <summary>
        /// Add information from the database like QID, Wording ID, and missing series members.
        /// </summary>
        // each question candidate can be New, Existing, Deleted
        // New questions: series 
        //                  start? take all wordings as is
        //                  middle? for any empty fields, take from 'a' 
        //                nonseries - take all wordings as is

        // existing questions:
        //                  series
        //                      start? take all wordings as is
        //                      middle? for any empty fields, take from 'a'
        //                  nonseries - take all wordings as is

        // deleted questions: take all wordings as is
        private void EnrichCandidate(
            QuestionCandidatePreview candidate,
            List<QuestionCandidatePreview> questions,
            List<SurveyQuestion> existing,
            List<Person> people,
            List<CommentType> commentTypes,
            string sourceFilePath)
        {

            // find the db version of the candidate
            var matchingQuestion = existing.FirstOrDefault(x => x.VarName.VarName.Equals(candidate.VarName));
            if (matchingQuestion != null)
                candidate.QID = matchingQuestion.ID;

            // set the missing series parts 
            SetSeriesParts(candidate, questions, existing);

            // set the wording numbers for each wording
            SetWordingSet(candidate.Original);
            SetWordingSet(candidate.Revised);

            // fill in comment data
            foreach (var comment in candidate.Comments)
            {
                comment.Survey= candidate.Survey;
                comment.VarName = candidate.VarName;
                comment.Author.ID = MatchPerson(comment.Author.Name, people)?.ID ?? 0;
                comment.Authority.ID = MatchPerson(comment.Authority.Name, people)?.ID ?? 0;
                comment.NoteType.ID = commentTypes.FirstOrDefault(c => c.TypeName == comment.NoteType.TypeName)?.ID ?? 0;
                comment.Source ??= System.IO.Path.GetFileName(sourceFilePath);
            }
        }

        private void SetWordingSet(QuestionCandidate qc)
        {
            SetWording(qc.PreP, _wordingDataService.PreP);
            SetWording(qc.PreI, _wordingDataService.PreI);
            SetWording(qc.PreA, _wordingDataService.PreA);
            SetWording(qc.LitQ, _wordingDataService.LitQ);
            SetWording(qc.PstI, _wordingDataService.PstI);
            SetWording(qc.PstP, _wordingDataService.PstP);
            SetResponseSet(qc.RespOptions, _wordingDataService.RO);
            SetResponseSet(qc.NRCodes, _wordingDataService.NR);
        }

       
        public async Task SetWording(WordingCandidate candidate)
        {
            var masterlist = await GetMasterListFor(candidate.FieldName);
            SetWording(candidate, masterlist);
        }

        public async Task SetResponseSet(ResponseSetCandidate candidate)
        {
            var masterlist = await GetMasterListFor(candidate.FieldName);
            SetResponseSet(candidate, masterlist);
        }

        public void SetWording(WordingCandidate candidate, IEnumerable<Wording> master)
        {
            var match = master.FirstOrDefault(w => LinesMatch(w.WordingText, candidate.Text));
            candidate.NewWording = match == null;
            candidate.WordID = match?.WordID ?? -1;
        }

      
        public void SetResponseSet(ResponseSetCandidate candidate, IEnumerable<ResponseSet> master)
        {
            var match = master.FirstOrDefault(r => LinesMatch(r.RespList, candidate.Text));
            candidate.NewWording = match == null;
            candidate.SetName = match?.RespSetName ?? GenerateUniqueName(master);
        }

        private string GenerateUniqueName(IEnumerable<ResponseSet> list)
        {
            var tmp = new ResponseSet();
            do tmp.SetRandomName();
            while (list.Any(x => x.RespSetName == tmp.RespSetName));
            return tmp.RespSetName;
        }

        private Person? MatchPerson(string name, List<Person> people) =>
            people.FirstOrDefault(p => p.Name == name || ($"{p.FirstName} {p.LastName}" == name));

        /// <summary>
        /// Returns true if all non-empty lines match between 2 strings.
        /// </summary>
        /// <param name="text1"></param>
        /// <param name="text2"></param>
        /// <returns></returns>
        bool LinesMatch(string text1, string text2)
        {
            // split on <br> tags
            var lines1 = text1.Split("<br>");
            var lines2 = text2.Split("<br>");
            // check if the number of substrings is equal
            if (lines1.Length != lines2.Length)
                return false;

            // Compare each non-empty substring after trimming
            for (int i = 0; i < lines1.Length; i++)
            {
                string trimmed1 = System.Net.WebUtility.HtmlDecode(lines1[i].Trim());
                string trimmed2 = System.Net.WebUtility.HtmlDecode(lines2[i].Trim());

                // If the trimmed substrings are not equal, return false
                if (trimmed1 != trimmed2)
                    return false;
            }

            // All non-empty substrings are equal
            return true;
        }

        private void SetSeriesParts(QuestionCandidatePreview questionCandidate, List<QuestionCandidatePreview> questions, List<SurveyQuestion> existing)
        {
            var matchingQuestion = existing.FirstOrDefault(x => x.VarName.VarName.Equals(questionCandidate.VarName));
            if (matchingQuestion != null && !string.IsNullOrEmpty(questionCandidate.Qnum) && char.IsLetter(questionCandidate.Qnum[questionCandidate.Qnum.Length - 1]) && questionCandidate.Qnum[questionCandidate.Qnum.Length - 1] != 'a')
            {
                if (string.IsNullOrEmpty(questionCandidate.Original.PreP.Text)) questionCandidate.Original.PreP = new WordingCandidate(WordingType.PreP, matchingQuestion.PrePW.WordID, matchingQuestion.PrePW.WordingText);
                if (string.IsNullOrEmpty(questionCandidate.Original.PreI.Text)) questionCandidate.Original.PreI = new WordingCandidate(WordingType.PreI, matchingQuestion.PreIW.WordID, matchingQuestion.PreIW.WordingText);
                if (string.IsNullOrEmpty(questionCandidate.Original.PreA.Text)) questionCandidate.Original.PreA = new WordingCandidate(WordingType.PreA, matchingQuestion.PreAW.WordID, matchingQuestion.PreAW.WordingText);
                if (string.IsNullOrEmpty(questionCandidate.Original.RespOptions.Text)) questionCandidate.Original.RespOptions = new ResponseSetCandidate(ResponseType.RespOptions, matchingQuestion.RespOptionsS.RespSetName, matchingQuestion.RespOptionsS.RespList);
                if (string.IsNullOrEmpty(questionCandidate.Original.NRCodes.Text)) questionCandidate.Original.NRCodes = new ResponseSetCandidate(ResponseType.NRCodes, matchingQuestion.NRCodesS.RespSetName, matchingQuestion.NRCodesS.RespList);
                if (string.IsNullOrEmpty(questionCandidate.Original.PstI.Text)) questionCandidate.Original.PstI = new WordingCandidate(WordingType.PstI, matchingQuestion.PstIW.WordID, matchingQuestion.PstIW.WordingText);
                if (string.IsNullOrEmpty(questionCandidate.Original.PstP.Text)) questionCandidate.Original.PstP = new WordingCandidate(WordingType.PstP, matchingQuestion.PstPW.WordID, matchingQuestion.PstPW.WordingText);

                if (!questionCandidate.IsDeletion)
                {
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PreP.Text)) questionCandidate.Revised.PreP = new WordingCandidate(WordingType.PreP, matchingQuestion.PrePW.WordID, matchingQuestion.PrePW.WordingText);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PreI.Text)) questionCandidate.Revised.PreI = new WordingCandidate(WordingType.PreI, matchingQuestion.PreIW.WordID, matchingQuestion.PreIW.WordingText);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PreA.Text)) questionCandidate.Revised.PreA = new WordingCandidate(WordingType.PreA, matchingQuestion.PreAW.WordID, matchingQuestion.PreAW.WordingText);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.RespOptions.Text)) questionCandidate.Revised.RespOptions = new ResponseSetCandidate(ResponseType.RespOptions, matchingQuestion.RespOptionsS.RespSetName, matchingQuestion.RespOptionsS.RespList);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.NRCodes.Text)) questionCandidate.Revised.NRCodes = new ResponseSetCandidate(ResponseType.NRCodes, matchingQuestion.NRCodesS.RespSetName, matchingQuestion.NRCodesS.RespList);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PstI.Text)) questionCandidate.Revised.PstI = new WordingCandidate(WordingType.PstI, matchingQuestion.PstIW.WordID, matchingQuestion.PstIW.WordingText);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PstP.Text)) questionCandidate.Revised.PstP = new WordingCandidate(WordingType.PstP, matchingQuestion.PstPW.WordID, matchingQuestion.PstPW.WordingText);
                }
            }

            if (questionCandidate.IsNewQuestion && char.IsLetter(questionCandidate.Qnum[questionCandidate.Qnum.Length - 1]))
            {
                var seriesStarter = existing.FirstOrDefault(x => x.Qnum.Substring(0, 3).Equals(questionCandidate.Qnum.Substring(0, 3)));
                if (seriesStarter != null)
                {
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PreP.Text)) questionCandidate.Revised.PreP = new WordingCandidate(WordingType.PreP, seriesStarter.PrePW.WordID, seriesStarter.PrePW.WordingText);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PreI.Text)) questionCandidate.Revised.PreI = new WordingCandidate(WordingType.PreI, seriesStarter.PreIW.WordID, seriesStarter.PreIW.WordingText);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PreA.Text)) questionCandidate.Revised.PreA = new WordingCandidate(WordingType.PreA, seriesStarter.PreAW.WordID, seriesStarter.PreAW.WordingText);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.RespOptions.Text)) questionCandidate.Revised.RespOptions = new ResponseSetCandidate(ResponseType.RespOptions, seriesStarter.RespOptionsS.RespSetName, seriesStarter.RespOptionsS.RespList);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.NRCodes.Text)) questionCandidate.Revised.NRCodes = new ResponseSetCandidate(ResponseType.NRCodes, seriesStarter.NRCodesS.RespSetName, seriesStarter.NRCodesS.RespList);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PstI.Text)) questionCandidate.Revised.PstI = new WordingCandidate(WordingType.PstI, seriesStarter.PstIW.WordID, seriesStarter.PstIW.WordingText);
                    if (string.IsNullOrEmpty(questionCandidate.Revised.PstP.Text)) questionCandidate.Revised.PstP = new WordingCandidate(WordingType.PstP, seriesStarter.PstPW.WordID, seriesStarter.PstPW.WordingText);
                }
                else
                {
                    var seriesStarter2 = questions.FirstOrDefault(x => x.Qnum.Substring(0, 3).Equals(questionCandidate.Qnum.Substring(0, 3)));
                    if (seriesStarter2 != null)
                    {
                        if (string.IsNullOrEmpty(questionCandidate.Revised.PreP.Text)) questionCandidate.Revised.PreP = new WordingCandidate(WordingType.PreP, seriesStarter2.Revised.PreP.WordID, seriesStarter2.Revised.PreP.Text);
                        if (string.IsNullOrEmpty(questionCandidate.Revised.PreI.Text)) questionCandidate.Revised.PreI = new WordingCandidate(WordingType.PreI, seriesStarter2.Revised.PreI.WordID, seriesStarter2.Revised.PreI.Text);
                        if (string.IsNullOrEmpty(questionCandidate.Revised.PreA.Text)) questionCandidate.Revised.PreA = new WordingCandidate(WordingType.PreA, seriesStarter2.Revised.PreA.WordID, seriesStarter2.Revised.PreA.Text);
                        if (string.IsNullOrEmpty(questionCandidate.Revised.RespOptions.Text)) questionCandidate.Revised.RespOptions = new ResponseSetCandidate(ResponseType.RespOptions, seriesStarter2.Revised.RespOptions.SetName, seriesStarter2.Revised.RespOptions.Text);
                        if (string.IsNullOrEmpty(questionCandidate.Revised.NRCodes.Text)) questionCandidate.Revised.NRCodes = new ResponseSetCandidate(ResponseType.NRCodes, seriesStarter2.Revised.NRCodes.SetName, seriesStarter2.Revised.NRCodes.Text);
                        if (string.IsNullOrEmpty(questionCandidate.Revised.PstI.Text)) questionCandidate.Revised.PstI = new WordingCandidate(WordingType.PstI, seriesStarter2.Revised.PstI.WordID, seriesStarter2.Revised.PstI.Text);
                        if (string.IsNullOrEmpty(questionCandidate.Revised.PstP.Text)) questionCandidate.Revised.PstP = new WordingCandidate(WordingType.PstP, seriesStarter2.Revised.PstP.WordID, seriesStarter2.Revised.PstP.Text);
                    }
                }
            }
        }
    }



    
}
