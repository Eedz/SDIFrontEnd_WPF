using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using ITCLib;
using ITC_Contracts;
using System.Net.Http.Json;
using SDIFrontEnd_WPF.Mappings;
namespace SDIFrontEnd_WPF
{
    public class ApiCommentService : ApiServiceBase, IApiCommentService
    {
        private readonly IMapper<QuestionComment, QuestionCommentDto> _questionCommentMapper;
        private readonly IMapper<DeletedComment, DeletedCommentDto> _deletedCommentMapper;

        public ApiCommentService(HttpClient httpClient, IMapperFactory mapperFactory ) : base(httpClient)
        {
            _questionCommentMapper = mapperFactory.Get<QuestionComment, QuestionCommentDto>();
            _deletedCommentMapper = mapperFactory.Get<DeletedComment, DeletedCommentDto>();
        }

        public async Task<List<QuestionComment>> GetQuestionCommentsAsync(int questionId)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<QuestionCommentDto>>($"{_baseApi}/comments/question/{questionId}");

                var comments = response.Select(c => _questionCommentMapper.MapToEntity(c)).ToList();
                return comments;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new ApplicationException($"Error fetching comments for question {questionId}: {ex.Message}", ex);
            }
        }

        public async Task<List<CommentType>> GetCommentTypesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<CommentTypeDto>>($"{_baseApi}/comments/types");
                var commentTypes = response.Select(ct => new CommentType() { ID = ct.ID, TypeName = ct.TypeName, ShortForm = ct.ShortForm }).ToList();
                return commentTypes;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new ApplicationException($"Error fetching comment types: {ex.Message}", ex);
            }
        }

        public async Task<int> AddCommentAsync(QuestionComment comment)
        {
            try
            {
                var commentDto = _questionCommentMapper.MapToDto(comment);
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
                var response = await _http.PostAsync($"{_baseApi}/comments/backup/{qid}", null);
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
                var commentDto = _deletedCommentMapper.MapToDto(comment);
                var response = await _http.PostAsJsonAsync($"{_baseApi}/comments/deleted", commentDto);
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
