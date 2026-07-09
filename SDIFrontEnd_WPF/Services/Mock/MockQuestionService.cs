
using ITC_Contracts;
using ITCLib;
using SDIFrontEnd_WPF.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace SDIFrontEnd_WPF
{
    public class MockQuestionService : IApiQuestionService
    {
        public MockQuestionService()
        {

        }

        public async Task<SurveyQuestion?> GetQuestionByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<SurveyQuestion>> GetQuestionsByVarNameAsync(string varname)
        {
            throw new NotImplementedException();
        }

        public async Task<List<SurveyQuestion>> GetQuestionsByRefVarNameAsync(string varname)
        {
            throw new NotImplementedException();
        }

        public async Task<SurveyQuestionRecord> SaveQuestion(SurveyQuestionRecord question)
        {
            throw new NotImplementedException();
        }

        public async Task<List<SurveyQuestion>> SearchQuestions(string searchTerm)
        {
            throw new NotImplementedException();
        }

        public async Task<int> AddQuestion(SurveyQuestion question)
        {
            throw new NotImplementedException();
        }

        public async Task<int> UpdateQuestion(SurveyQuestion question)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteQuestion(SurveyQuestion question)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateQnums(IEnumerable<SurveyQuestion> qnums)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> CreateTranslation(Translation translation)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateTranslation(Translation translation)
        {
            throw new NotImplementedException();
        }

        private SurveyQuestion MapToEntity(SurveyQuestionDto dto)
        {
            return new SurveyQuestion
            {
                VarName = new VariableName(dto.VarName.VarName)
                {
                    VarLabel = dto.VarName.VarLabel,
                    Domain = new DomainLabel() { LabelText = dto.VarName.Domain.LabelText, ID = dto.VarName.Domain.ID },
                    Topic = new TopicLabel() { LabelText = dto.VarName.Topic.LabelText, ID = dto.VarName.Topic.ID },
                    Content = new ContentLabel() { LabelText = dto.VarName.Content.LabelText, ID = dto.VarName.Content.ID },
                    Product = new ProductLabel() { LabelText = dto.VarName.Product.LabelText, ID = dto.VarName.Product.ID },
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
            };
        }

        private SurveyQuestionDto MapToDto(SurveyQuestion question)
        {
            return new SurveyQuestionDto
            {
                VarName = new VariableNameDto
                {
                    VarName = question.VarName.VarName,
                    VarLabel = question.VarName.VarLabel,
                    Domain = new VarNameLabelDto { LabelText = question.VarName.Domain.LabelText, ID = question.VarName.Domain.ID },
                    Topic = new VarNameLabelDto { LabelText = question.VarName.Topic.LabelText, ID = question.VarName.Topic.ID },
                    Content = new VarNameLabelDto { LabelText = question.VarName.Content.LabelText, ID = question.VarName.Content.ID },
                    Product = new VarNameLabelDto { LabelText = question.VarName.Product.LabelText, ID = question.VarName.Product.ID },
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
