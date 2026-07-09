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
    public class MockCommentService : IApiCommentService
    {
        public MockCommentService()
        {
        }

        public async Task<List<QuestionComment>> GetQuestionCommentsAsync(int questionId)
        {
            var comments = await Task.FromResult(new List<QuestionComment>
            {
                new QuestionComment
                {
                    QID = questionId,
                    Notes = new Note { ID = 1, NoteText = "This is a comment for question " + questionId },
                    NoteDate = DateTime.Now,
                    Author = new Person { ID = 1, Name = "John Doe" },
                    Authority = new Person { ID = 2, Name = "Jane Smith" },
                    Source = "System",
                    NoteType = new CommentType { ID = 1, TypeName = "General", ShortForm = "GEN" }
                }
            });
            return comments;
        }

        public async Task<int> AddCommentAsync(QuestionComment comment)
        {
            return 1;
        }

        public async Task BackupCommentsAsync(int qid)
        {
            
        }

        public async Task<bool> InsertDeletedComment(DeletedComment comment)
        {
            return true;
        }

        public Task<List<CommentType>> GetCommentTypesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
