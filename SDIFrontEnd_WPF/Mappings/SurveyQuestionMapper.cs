using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITC_Contracts;
using ITCLib;

namespace SDIFrontEnd_WPF.Mappings
{
    public class SurveyQuestionMapper : IMapper<SurveyQuestion,SurveyQuestionDto>
    {
        private readonly IMapper<QuestionComment, QuestionCommentDto> _commentMapper;

        public SurveyQuestionMapper(IMapperFactory mapperFactory) 
        { 
            _commentMapper = mapperFactory.Get<QuestionComment, QuestionCommentDto>();
        }

        public SurveyQuestion MapToEntity(SurveyQuestionDto dto)
        {
            return new SurveyQuestion
            {
                ID = dto.ID,
                SurveyCode = dto.SurveyCode,
                VarName = new VariableName(dto.VarName.VarName)
                {
                    ID = dto.VarName.ID,
                    VarLabel = dto.VarName.VarLabel,
                    Domain = new DomainLabel() { LabelText = dto.VarName.Domain.LabelText, ID = dto.VarName.Domain.ID },
                    Topic = new TopicLabel() { LabelText = dto.VarName.Topic.LabelText, ID = dto.VarName.Topic.ID },
                    Content = new ContentLabel() { LabelText = dto.VarName.Content.LabelText, ID = dto.VarName.Content.ID },
                    Product = new ProductLabel() { LabelText = dto.VarName.Product.LabelText, ID = dto.VarName.Product.ID },
                    DomainLabel = new VarNameLabel(dto.VarName.Domain.ID, dto.VarName.Domain.LabelText),
                    TopicLabel = new VarNameLabel(dto.VarName.Topic.ID, dto.VarName.Topic.LabelText),
                    ContentLabel = new VarNameLabel(dto.VarName.Content.ID, dto.VarName.Content.LabelText),
                    ProductLabel = new VarNameLabel(dto.VarName.Product.ID, dto.VarName.Product.LabelText)
                },
                Qnum = dto.Qnum,
                AltQnum = dto.AltQnum,
                PrePW = dto.PrePW == null ? null : new Wording() { WordID = dto.PrePW.ID, WordingText = dto.PrePW.WordingText },
                PreIW = dto.PreIW == null ? null : new Wording() { WordID = dto.PreIW.ID, WordingText = dto.PreIW.WordingText },
                PreAW = dto.PreAW == null ? null : new Wording() { WordID = dto.PreAW.ID, WordingText = dto.PreAW.WordingText },
                LitQW = dto.LitQW == null ? null : new Wording() { WordID = dto.LitQW.ID, WordingText = dto.LitQW.WordingText },
                PstIW = dto.PstIW == null ? null : new Wording() { WordID = dto.PstIW.ID, WordingText = dto.PstIW.WordingText },
                PstPW = dto.PstPW == null ? null : new Wording() { WordID = dto.PstPW.ID, WordingText = dto.PstPW.WordingText },
                RespOptionsS = dto.RespOptionsS == null ? null : new ResponseSet() { RespSetName = dto.RespOptionsS.RespSetName, RespList = dto.RespOptionsS.RespList },
                NRCodesS = dto.NRCodesS == null ? null : new ResponseSet() { RespSetName = dto.NRCodesS.RespSetName, RespList = dto.NRCodesS.RespList },

                Comments = dto.Comments.Select(x=> _commentMapper.MapToEntity(x)).ToList(),
                Translations = dto.Translations.Select(x => new Translation() { ID = x.ID, QID = x.QID, Bilingual = x.Bilingual, LanguageName = new Language() { LanguageName = x.LanguageName }, TranslationText = x.TranslationText }).ToList(),
                TimeFrames = dto.TimeFrames.Select(x => new QuestionTimeFrame() { ID = x.ID, QID = x.QID, TimeFrame = x.TimeFrame }).ToList(),
                Images = dto.Images.Select(x=>new SurveyImage() { ID = x.ID, QID = x.QID, FilePath = x.FilePath, ImagePath = x.FilePath, ImageName = x.ImageName }).ToList()
            };
        }



        public SurveyQuestionDto MapToDto(SurveyQuestion question)
        {
            return new SurveyQuestionDto
            {
                ID = question.ID,
                SurveyCode = question.SurveyCode,
                VarName = new VariableNameDto
                {
                    ID = question.VarName.ID,
                    VarName = question.VarName.VarName,
                    VarLabel = question.VarName.VarLabel,
                    Domain = new VarNameLabelDto { LabelText = question.VarName.DomainLabel.Label, ID = question.VarName.DomainLabel.ID },
                    Topic = new VarNameLabelDto { LabelText = question.VarName.TopicLabel.Label, ID = question.VarName.TopicLabel.ID },
                    Content = new VarNameLabelDto { LabelText = question.VarName.ContentLabel.Label, ID = question.VarName.ContentLabel.ID },
                    Product = new VarNameLabelDto { LabelText = question.VarName.ProductLabel.Label, ID = question.VarName.ProductLabel.ID },
                },
                Qnum = question.Qnum,
                AltQnum = question.AltQnum,
                PrePW = question.PrePW == null ? null : new WordingDto { ID = question.PrePW.WordID, WordingText = question.PrePW.WordingText },
                PreIW = question.PreIW == null ? null : new WordingDto { ID = question.PreIW.WordID, WordingText = question.PreIW.WordingText },
                PreAW = question.PreAW == null ? null : new WordingDto { ID = question.PreAW.WordID, WordingText = question.PreAW.WordingText },
                LitQW = question.LitQW == null ? null : new WordingDto { ID = question.LitQW.WordID, WordingText = question.LitQW.WordingText },
                PstIW = question.PstIW == null ? null : new WordingDto { ID = question.PstIW.WordID, WordingText = question.PstIW.WordingText },
                PstPW = question.PstPW == null ? null : new WordingDto { ID = question.PstPW.WordID, WordingText = question.PstPW.WordingText },
                RespOptionsS = question.RespOptionsS == null ? null : new ResponseSetDto { RespSetName = question.RespOptionsS.RespSetName, RespList = question.RespOptionsS.RespList },
                NRCodesS = question.NRCodesS == null ? null : new ResponseSetDto { RespSetName = question.NRCodesS.RespSetName, RespList = question.NRCodesS.RespList },
            };
        }
    }
}
