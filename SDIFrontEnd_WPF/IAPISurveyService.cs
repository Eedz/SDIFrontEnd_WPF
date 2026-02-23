using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;

namespace SDIFrontEnd_WPF
{
    public interface IApiSurveyService : IApiService
    {
        Task<List<Survey>> GetAllAsync();
        Task<List<StudyWave>> GetAllWavesAsync();
        Task<List<Study>> GetAllStudiesAsync();
        Task<Survey?> GetSurveyByIdAsync(int id);

        Task<List<SurveyQuestion>> FindQuestionsByRefVarName(string refVarName);
    }
}
