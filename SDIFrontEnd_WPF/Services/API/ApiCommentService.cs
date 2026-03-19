using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using ITCLib;
using ITC_Contracts;
using System.Net.Http.Json;
namespace SDIFrontEnd_WPF
{
    public class ApiCommentService : ApiServiceBase, IApiCommentService
    {
        public ApiCommentService(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<List<QuestionComment>> GetQuestionCommentsAsync(int questionId)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<QuestionCommentDto>>($"api/comments/question/{questionId}");

                var comments = response.Select(c => new QuestionComment
                {
                    ID = c.ID,
                    QID = c.QID,
                    Notes = new Note(c.NoteID, c.NoteText),
                    NoteDate = c.NoteDate,
                    Author = new Person(c.Author.Name, c.Author.ID),
                    Authority = new Person(c.Authority.Name, c.Authority.ID),
                    Source = c.Source,
                    NoteType = new CommentType(c.NoteType.ID, c.NoteType.TypeName, c.NoteType.ShortForm),
                }).ToList();
                return comments;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new ApplicationException($"Error fetching comments for question {questionId}: {ex.Message}", ex);
            }
        }

        public async Task<int> AddCommentAsync(QuestionComment comment)
        {
            try
            {
                var commentDto = new QuestionCommentDto
                {
                    QID = comment.QID,
                    NoteID = comment.Notes.ID,
                    NoteText = comment.Notes.NoteText,
                    NoteDate = comment.NoteDate,
                    Author = new PersonDto { ID = comment.Author.ID, Name = comment.Author.Name },
                    Authority = new PersonDto { ID = comment.Authority.ID, Name = comment.Authority.Name },
                    Source = comment.Source,
                    NoteType = new CommentTypeDto { ID = comment.NoteType.ID, TypeName = comment.NoteType.TypeName, ShortForm = comment.NoteType.ShortForm }
                };
                var response = await _http.PostAsJsonAsync("api/comments", commentDto);
                response.EnsureSuccessStatusCode();
                var createdCommentId = await response.Content.ReadFromJsonAsync<int>();
                return createdCommentId;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new ApplicationException($"Error adding comment for question {comment.QID}: {ex.Message}", ex);
            }
        }

        public async Task BackupCommentsAsync(int qid)
        {
            try
            {
                var response = await _http.PostAsync($"api/comments/backup/{qid}", null);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new ApplicationException($"Error backing up comments for question {qid}: {ex.Message}", ex);
            }
        }

        public async Task<bool> InsertDeletedComment(DeletedComment comment)
        {
            try
            {
                var commentDto = new DeletedCommentDto
                {
                    Survey = comment.Survey,
                    VarName = comment.VarName,
                    SurvID = comment.SurvID,
                    NoteID = comment.Notes.ID,
                    NoteText = comment.Notes.NoteText,
                    NoteDate = comment.NoteDate,
                    Author = new PersonDto { ID = comment.Author.ID, Name = comment.Author.Name },
                    Authority = new PersonDto { ID = comment.Authority.ID, Name = comment.Authority.Name },
                    Source = comment.Source,
                    NoteType = new CommentTypeDto { ID = comment.NoteType.ID, TypeName = comment.NoteType.TypeName, ShortForm = comment.NoteType.ShortForm }
                };
                var response = await _http.PostAsJsonAsync("api/comments/deleted", commentDto);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new ApplicationException($"Error inserting deleted comment for question {comment.Survey}.{comment.VarName}: {ex.Message}", ex);
            }
        }
    }
}
