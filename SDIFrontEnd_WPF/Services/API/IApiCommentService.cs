using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;
namespace SDIFrontEnd_WPF
{
    public interface IApiCommentService
    {
        Task<List<QuestionComment>> GetQuestionCommentsAsync(int questionId);

        Task<int> AddCommentAsync(QuestionComment comment);
        Task BackupCommentsAsync(int qid);
        Task<bool> InsertDeletedComment(DeletedComment comment);
        Task<List<CommentType>> GetCommentTypesAsync();
    }
}
