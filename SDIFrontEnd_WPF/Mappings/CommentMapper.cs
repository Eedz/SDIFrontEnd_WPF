using ITC_Contracts;
using ITCLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDIFrontEnd_WPF.Mappings
{
    public class QuestionCommentMapper : IMapper<QuestionComment, QuestionCommentDto>
    {
        public QuestionCommentDto MapToDto(QuestionComment entity)
        {
            return new QuestionCommentDto
            {
                ID = entity.ID,
                QID = entity.QID,
                VarName = entity.VarName,
                Survey = entity.Survey,
                NoteID = entity.Notes.ID,
                NoteText = entity.Notes.NoteText,
                NoteDate = entity.NoteDate,
                Author = new PersonDto { ID = entity.Author.ID, Name = entity.Author.Name },
                Authority = new PersonDto { ID = entity.Authority.ID, Name = entity.Authority.Name },
                Source = entity.Source,
                NoteType = new CommentTypeDto { ID = entity.NoteType.ID, TypeName = entity.NoteType.TypeName, ShortForm = entity.NoteType.ShortForm }
            };
        }

        public QuestionComment MapToEntity(QuestionCommentDto dto)
        {
            return new QuestionComment
            {
                ID = dto.ID,
                QID = dto.QID,
                VarName = dto.VarName,
                Survey = dto.Survey,
                Notes = new Note(dto.NoteID, dto.NoteText),
                NoteDate = dto.NoteDate,
                Author = new Person(dto.Author.Name, dto.Author.ID),
                Authority = new Person(dto.Authority.Name, dto.Authority.ID),
                Source = dto.Source,
                NoteType = new CommentType(dto.NoteType.ID, dto.NoteType.TypeName, dto.NoteType.ShortForm),
            };
        }
    }

    public class DeletedCommentMapper : IMapper<DeletedComment, DeletedCommentDto>
    {
        public DeletedCommentDto MapToDto(DeletedComment entity)
        {
            return new DeletedCommentDto
            {
                ID = entity.ID,
                Survey = entity.Survey,
                VarName = entity.VarName,
                SurvID = entity.SurvID,
                NoteID = entity.Notes.ID,
                NoteText = entity.Notes.NoteText,
                NoteDate = entity.NoteDate,
                Author = new PersonDto { ID = entity.Author.ID, Name = entity.Author.Name },
                Authority = new PersonDto { ID = entity.Authority.ID, Name = entity.Authority.Name },
                Source = entity.Source,
                NoteType = new CommentTypeDto { ID = entity.NoteType.ID, TypeName = entity.NoteType.TypeName, ShortForm = entity.NoteType.ShortForm }
            };
        }

        public DeletedComment MapToEntity(DeletedCommentDto dto)
        {
            return new DeletedComment
            {
                ID = dto.ID,
                SurvID = dto.SurvID,
                Survey = dto.Survey,
                VarName = dto.VarName,
                Notes = new Note(dto.NoteID, dto.NoteText),
                NoteDate = dto.NoteDate,
                Author = new Person(dto.Author.Name, dto.Author.ID),
                Authority = new Person(dto.Authority.Name, dto.Authority.ID),
                Source = dto.Source,
                NoteType = new CommentType(dto.NoteType.ID, dto.NoteType.TypeName, dto.NoteType.ShortForm),
            };
        }
    }
}
